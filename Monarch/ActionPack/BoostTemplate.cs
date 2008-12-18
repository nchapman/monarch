using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Boo.Lang.Compiler;
using Boo.Lang.Compiler.IO;
using Boo.Lang.Compiler.Pipelines;
using Boo.Lang.Parser;

namespace Monarch.ActionPack
{
    public class BoostTemplate
    {
        #region Constants

        const RegexOptions brailRegexOptions = RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace;
        const RegexOptions velocityRegexOptions = RegexOptions.Compiled | RegexOptions.Multiline;
        const string velocityVariablePattern = @"[A-Za-z][A-Za-z0-9\.\[\]\""\'\{\}_]*";

        #endregion

        #region Readonly & Static Fields

        static readonly Regex velocityComment = new Regex(@"##.+$", velocityRegexOptions);
        static readonly Regex velocityDirectivesClose = new Regex(@"#end", velocityRegexOptions);
        static readonly Regex velocityDirectivesWithArguments = new Regex(@"#(set|if|elseif|foreach)\s*\(\s*(.+?)\s*\)\s*$", velocityRegexOptions);
        static readonly Regex velocityDirectivesWithoutArguments = new Regex(@"#(else)", velocityRegexOptions);
        static readonly Regex velocityFormalVariables = new Regex(@"(?<!<%.+(?=[^%]+%>))\$\{(" + velocityVariablePattern + @")\}(?!(?<=<%.+)[^%]+%>)", velocityRegexOptions);
        static readonly Regex velocityinformalVariables = new Regex(@"\$(" + velocityVariablePattern + @")", velocityRegexOptions);
        static readonly Regex velocityMultiLineComment = new Regex(@"#\*.+\*#", velocityRegexOptions);
        static readonly Regex velocityVariableInDirective = new Regex(@"\$(" + velocityVariablePattern + @")", velocityRegexOptions);

        static readonly Regex brailComment = new Regex(@"<%\# .*? (?<!%)%>", brailRegexOptions | RegexOptions.Singleline);
        readonly List<string> brailImportStatements = new List<string>();
        static readonly Regex brailInstruction = new Regex(@"<%(?!%) (.*?) (?<!%)%>", brailRegexOptions | RegexOptions.Singleline);
        static readonly Regex brailNewLines = new Regex(@"\r\n|\r", brailRegexOptions);
        
        readonly string viewSource;
        readonly string layoutSource;

        #endregion

        #region Fields

        BoostOutputter generatedOutputter;

        #endregion

        #region Constructors

        public BoostTemplate(string viewSource, string layoutSource)
        {
            this.viewSource = viewSource;
            this.layoutSource = layoutSource;
        }

        public BoostTemplate(string viewSource)
        {
            this.viewSource = viewSource;
            layoutSource = "<%= viewOutput %>";
        }

        #endregion

        #region Instance Methods

        public BoostOutputter Parse()
        {
            var parsedView = ParseVelocity(viewSource);
            var parsedLayout = ParseVelocity(layoutSource);

            parsedView = ParseBrail(parsedView);
            parsedLayout = ParseBrail(parsedLayout);

            var imports = string.Join(Environment.NewLine, brailImportStatements.ToArray()) + Environment.NewLine;

            var booClass = string.Format(@"{0}
            import Monarch.ActionPack

            class GeneratedOutputter(BoostTemplate.BoostOutputter):
	            private output as System.Text.StringBuilder

	            override def Run(data as ViewDictionary) as string:
		            output = System.Text.StringBuilder()
                    
                    # Output viewSource
		            {1}

                    viewOutput = output.ToString()

                    output = System.Text.StringBuilder()

                    # Output layoutSource
                    {2}

		            return output.ToString()
	            end
            end
            ", imports, parsedView, parsedLayout);

            return Compile(booClass);
        }

        public static string ParseVelocity(string source)
        {
            var parsedSource = source;

            parsedSource = brailNewLines.Replace(parsedSource, "\n");

            parsedSource = velocityDirectivesWithArguments.Replace(parsedSource, new MatchEvaluator(ParseVelocityDirective));
            parsedSource = velocityDirectivesWithoutArguments.Replace(parsedSource, "<% $1: %>");
            parsedSource = velocityDirectivesClose.Replace(parsedSource, "<% end %>");

            parsedSource = velocityFormalVariables.Replace(parsedSource, "<%= $1 %>");
            parsedSource = velocityinformalVariables.Replace(parsedSource, "<%= $1 %>");

            parsedSource = velocityComment.Replace(parsedSource, "");
            parsedSource = velocityMultiLineComment.Replace(parsedSource, "");

            return parsedSource;
        }

        public string Run()
        {
            return Run(new ViewDictionary());
        }

        public string Run(ViewDictionary data)
        {
            if (null == generatedOutputter)
                generatedOutputter = Parse();

            return generatedOutputter.Run(data);
        }

        string ParseBrail(string source)
        {
            var parsedSource = source;

            parsedSource = brailComment.Replace(parsedSource, "");

            var toReturn = new StringBuilder();

            var previousIndex = 0;

            foreach (Match match in brailInstruction.Matches(parsedSource))
            {
                if (previousIndex < match.Index)
                    toReturn.Append(ParseBrailLiteral(parsedSource.Substring(previousIndex, match.Index - previousIndex)));

                var instruction = match.Groups[1].Value;

                // Handle output directives
                if (instruction.StartsWith("="))
                {
                    var expression = instruction.Substring(1);
                    toReturn.Append(ParseBrailExpression(expression));
                }
                // Handle import directives
                else if (instruction.StartsWith("@"))
                {
                    var control = instruction.Substring(1);
                    brailImportStatements.Add(ParseBrailControlStatement(control));
                }
                // Handle code lines
                else
                {
                    toReturn.Append(ParseBrailCode(instruction));
                }

                previousIndex = match.Index + match.Length;
            }

            toReturn.Append(ParseBrailLiteral(parsedSource.Substring(previousIndex)));

            return toReturn.ToString();
        }

        #endregion

        #region Class Methods

        private static BoostOutputter Compile(string booSource)
        {
            var booc = new BooCompiler();

            booc.Parameters.Input.Add(new StringInput("Input", booSource));
            booc.Parameters.Ducky = true;

            booc.Parameters.References.Add(Assembly.GetAssembly(typeof(BoostOutputter)));

            booc.Parameters.Pipeline = new CompileToMemory();
            booc.Parameters.Pipeline[0] = new WSABooParsingStep();


            var output = new StringBuilder();

            var compilerContext = booc.Run();

            if (null == compilerContext.GeneratedAssembly)
            {
                foreach (CompilerError error in compilerContext.Errors)
                {
                    output.AppendLine(error.Message);
                }

                output.Append(booSource);
                throw new Exception(output.ToString());
            }

            var booClassType = compilerContext.GeneratedAssembly.GetType("GeneratedOutputter", true, true);

            return Activator.CreateInstance(booClassType) as BoostOutputter;
        }

        private static string EscapeBrailCode(string input)
        {
            var result = input;

            result = result.Replace("\\", "\\\\");
            result = result.Replace("'", "\\'");
            result = result.Replace("\r", "\\r");
            result = result.Replace("\n", "\\n");
            result = result.Replace("\t", "\\t");

            return result;
        }

        static string ParseBrailCode(string instruction)
        {
            return instruction.Trim() + Environment.NewLine;
        }

        static string ParseBrailControlStatement(string instruction)
        {
            return instruction.Trim();
        }

        static string ParseBrailExpression(string instruction)
        {
            return string.Format("output.Append({0}){1}", instruction.Trim(), Environment.NewLine);
        }

        static string ParseBrailLiteral(string literal)
        {
            if (literal == string.Empty)
                return string.Empty;

            var output = new StringBuilder();

            var lines = literal.Split(new[] { "\n" }, StringSplitOptions.None);

            for (var i = 0; i < lines.Length; i++)
            {
                var line = EscapeBrailCode(lines[i]);
                var method = i == lines.Length - 1 ? "Append" : "AppendLine";
                var code = string.Format("output.{0}('{1}')", method, line);

                output.AppendLine(code);
            }

            return output.ToString();
        }

        static string ParseBrailProperty(string name)
        {
            return string.Format("\t[property({0})]{1}\t_{0} as duck{1}{1}", name, Environment.NewLine);
        }

        static string ParseVelocityDirective(Match match)
        {
            var toReturn = new StringBuilder("<% ");
            var arguments = velocityVariableInDirective.Replace(match.Groups[2].Value, "$1");

            switch (match.Groups[1].Value)
            {
                case "set":
                    toReturn.Append(arguments);
                    break;
                case "if":
                    toReturn.AppendFormat("if({0}):", arguments);
                    break;
                case "elseif":
                    toReturn.AppendFormat("elif({0}):", arguments);
                    break;
                case "foreach":
                    toReturn.AppendFormat("for {0}:", arguments);
                    break;
            }

            toReturn.Append(" %>");

            return toReturn.ToString();
        }

        #endregion

        #region Nested type: BoostOutputter

        public class BoostOutputter
        {
            #region Instance Methods

            public virtual string Run(ViewDictionary data)
            {
                throw new NotImplementedException();
            }

            #endregion
        }

        #endregion
    }
}
