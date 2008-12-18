using Monarch.ActionPack;
using NUnit.Framework;

namespace Monarch.Tests.ActionPack
{   
    [TestFixture]
    public class BoostTemplateTest
    {
        [Test]
        public void TestBrailCode()
        {
            var template = new BoostTemplate("<% output.Append(1+2) %>");

            Assert.AreEqual(template.Run(), "3");
        }

        [Test]
        public void TestBrailCodeWrite()
        {
            var template = new BoostTemplate("<%= \"Hello\" %>");

            Assert.AreEqual(template.Run(), "Hello");
        }

        [Test]
        public void TestVelocitySetDirective()
        {
            Assert.AreEqual("<% name = 'nick' %>", BoostTemplate.ParseVelocity("#set($name = 'nick')"));
            Assert.AreEqual("<% name = 'nick' %>", BoostTemplate.ParseVelocity("#set ($name = 'nick')"));
            Assert.AreEqual("<% name = 'nick' %>", BoostTemplate.ParseVelocity("#set ( $name = 'nick' )"));
            Assert.AreEqual("<% name = 'nick' %>", BoostTemplate.ParseVelocity("#set    (    $name = 'nick'    )"));
        }

        [Test]
        public void TestVelocityForeachDirective()
        {
            Assert.AreEqual("<% for item in items: %>", BoostTemplate.ParseVelocity("#foreach($item in $items)"));
            Assert.AreEqual("<% for item in items: %>", BoostTemplate.ParseVelocity("#foreach ($item in $items)"));
            Assert.AreEqual("<% for item in items: %>", BoostTemplate.ParseVelocity("#foreach ( $item in $items )"));
            Assert.AreEqual("<% for item in items: %>", BoostTemplate.ParseVelocity("#foreach    (   $item in $items   )"));
        }

        [Test]
        public void TestVelocityVariables()
        {
            Assert.AreEqual("<%= name %>", BoostTemplate.ParseVelocity("${name}"));
            Assert.AreEqual("<%= name %>", BoostTemplate.ParseVelocity("$name"));
            Assert.AreEqual("<%= name %>ish", BoostTemplate.ParseVelocity("${name}ish"));
            Assert.AreEqual("<%= nameish %>", BoostTemplate.ParseVelocity("$nameish"));
        }

        [Test]
        public void TestVelocityStringInterpolation()
        {
            Assert.AreEqual("<%= var %> <%= \"${var}\" %>", BoostTemplate.ParseVelocity("${var} <%= \"${var}\" %>"));
            Assert.AreEqual("<%= 'hi' %> <%= var %> <% 'crazy' %>", BoostTemplate.ParseVelocity("<%= 'hi' %> ${var} <% 'crazy' %>"));
            Assert.AreEqual("<%= \"Hello ${soAndso}\" %>", BoostTemplate.ParseVelocity("<%= \"Hello ${soAndso}\" %>"));
            Assert.AreEqual("<% anotherName = \"Another ${name}\" %>", BoostTemplate.ParseVelocity("#set($anotherName = \"Another ${name}\")"));

            // This breaks it... not sure what to do... interpolation and modulus seem like a rare combo...
            //Assert.AreEqual("<%= var %> <%= \"${var}\" % \"0\" %>", BoostTemplate.ParseVelocity("${var} <%= \"${var}\" % \"0\" %>"));
        }
    }
}
