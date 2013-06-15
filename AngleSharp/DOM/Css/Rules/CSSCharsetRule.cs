﻿using System;

namespace AngleSharp.DOM.Css
{
    /// <summary>
    /// Represents the CSS @charset rule.
    /// </summary>
    sealed class CSSCharsetRule : CSSRule
    {
        #region ctor

        internal CSSCharsetRule()
        {
            _type = CssRule.Charset;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the encoding information set by this rule.
        /// </summary>
        public String Encoding { get; internal set; }

        #endregion
    }
}
