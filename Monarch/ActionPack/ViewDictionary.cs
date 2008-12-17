using System;
using System.Collections.Generic;
using Boo.Lang;

namespace Monarch.ActionPack
{
    public class ViewDictionary : Dictionary<string, object>, IQuackFu
    {
        #region IQuackFu Members

        public object QuackGet(string name, object[] parameters)
        {
            if (ContainsKey(name))
                return this[name];

            throw new InvalidOperationException("Property ${name} not found.");
        }

        public object QuackSet(string name, object[] parameters, object value)
        {
            throw new InvalidOperationException("Property ${name} can't be set.");
        }

        public object QuackInvoke(string name, params object[] args)
        {
            throw new InvalidOperationException("Method ${name} not found.");
        }

        #endregion
    }
}
