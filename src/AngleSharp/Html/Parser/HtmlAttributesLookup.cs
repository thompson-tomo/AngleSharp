namespace AngleSharp.Html.Parser;

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using Common;

static class HtmlAttributesLookup
{
    private static readonly Dictionary<StringOrMemory, String> WellKnownAttributeNames =
        new Dictionary<StringOrMemory, String>(OrdinalStringOrMemoryComparer.Instance)
        {
            { "name", "name" },
            { "http-equiv", "http-equiv" },
            { "scheme", "scheme" },
            { "content", "content" },
            { "class", "class" },
            { "style", "style" },
            { "label", "label" },
            { "action", "action" },
            { "prompt", "prompt" },
            { "href", "href" },
            { "hreflang", "hreflang" },
            { "lang", "lang" },
            { "disabled", "disabled" },
            { "selected", "selected" },
            { "value", "value" },
            { "title", "title" },
            { "type", "type" },
            { "rel", "rel" },
            { "rev", "rev" },
            { "accesskey", "accesskey" },
            { "download", "download" },
            { "media", "media" },
            { "target", "target" },
            { "charset", "charset" },
            { "alt", "alt" },
            { "coords", "coords" },
            { "shape", "shape" },
            { "formaction", "formaction" },
            { "formmethod", "formmethod" },
            { "formtarget", "formtarget" },
            { "formenctype", "formenctype" },
            { "formnovalidate", "formnovalidate" },
            { "dirname", "dirname" },
            { "dir", "dir" },
            { "nonce", "nonce" },
            { "noresize", "noresize" },
            { "src", "src" },
            { "srcset", "srcset" },
            { "srclang", "srclang" },
            { "srcdoc", "srcdoc" },
            { "scrolling", "scrolling" },
            { "longdesc", "longdesc" },
            { "frameborder", "frameborder" },
            { "width", "width" },
            { "height", "height" },
            { "marginwidth", "marginwidth" },
            { "marginheight", "marginheight" },
            { "cols", "cols" },
            { "rows", "rows" },
            { "align", "align" },
            { "encoding", "encoding" },
            { "standalone", "standalone" },
            { "version", "version" },
            { "dropzone", "dropzone" },
            { "draggable", "draggable" },
            { "spellcheck", "spellcheck" },
            { "tabindex", "tabindex" },
            { "contenteditable", "contenteditable" },
            { "translate", "translate" },
            { "contextmenu", "contextmenu" },
            { "hidden", "hidden" },
            { "id", "id" },
            { "sizes", "sizes" },
            { "scoped", "scoped" },
            { "reversed", "reversed" },
            { "start", "start" },
            { "ping", "ping" },
            { "ismap", "ismap" },
            { "usemap", "usemap" },
            { "crossorigin", "crossorigin" },
            { "sandbox", "sandbox" },
            { "allowfullscreen", "allowfullscreen" },
            { "allowpaymentrequest", "allowpaymentrequest" },
            { "data", "data" },
            { "typemustmatch", "typemustmatch" },
            { "autofocus", "autofocus" },
            { "accept-charset", "accept-charset" },
            { "enctype", "enctype" },
            { "autocomplete", "autocomplete" },
            { "method", "method" },
            { "novalidate", "novalidate" },
            { "for", "for" },
            { "seamless", "seamless" },
            { "valign", "valign" },
            { "span", "span" },
            { "bgcolor", "bgcolor" },
            { "colspan", "colspan" },
            { "referrerpolicy", "referrerpolicy" },
            { "rowspan", "rowspan" },
            { "nowrap", "nowrap" },
            { "abbr", "abbr" },
            { "scope", "scope" },
            { "headers", "headers" },
            { "axis", "axis" },
            { "border", "border" },
            { "cellpadding", "cellpadding" },
            { "rules", "rules" },
            { "summary", "summary" },
            { "cellspacing", "cellspacing" },
            { "frame", "frame" },
            { "form", "form" },
            { "required", "required" },
            { "multiple", "multiple" },
            { "min", "min" },
            { "max", "max" },
            { "open", "open" },
            { "challenge", "challenge" },
            { "keytype", "keytype" },
            { "size", "size" },
            { "wrap", "wrap" },
            { "maxlength", "maxlength" },
            { "minlength", "minlength" },
            { "placeholder", "placeholder" },
            { "readonly", "readonly" },
            { "accept", "accept" },
            { "pattern", "pattern" },
            { "step", "step" },
            { "list", "list" },
            { "checked", "checked" },
            { "kind", "kind" },
            { "default", "default" },
            { "autoplay", "autoplay" },
            { "preload", "preload" },
            { "loop", "loop" },
            { "controls", "controls" },
            { "muted", "muted" },
            { "mediagroup", "mediagroup" },
            { "poster", "poster" },
            { "color", "color" },
            { "face", "face" },
            { "command", "command" },
            { "icon", "icon" },
            { "radiogroup", "radiogroup" },
            { "cite", "cite" },
            { "async", "async" },
            { "defer", "defer" },
            { "language", "language" },
            { "event", "event" },
            { "alink", "alink" },
            { "background", "background" },
            { "link", "link" },
            { "text", "text" },
            { "vlink", "vlink" },
            { "show", "show" },
            { "role", "role" },
            { "actuate", "actuate" },
            { "arcrole", "arcrole" },
            { "space", "space" },
            { "window", "window" },
            { "manifest", "manifest" },
            { "datetime", "datetime" },
            { "low", "low" },
            { "high", "high" },
            { "optimum", "optimum" },
            { "slot", "slot" },
            { "body", "body" },
            { "integrity", "integrity" },
            { "clear", "clear" },
            { "codetype", "codetype" },
            { "compact", "compact" },
            { "declare", "declare" },
            { "direction", "direction" },
            { "nohref", "nohref" },
            { "noshade", "noshade" },
            { "valuetype", "valuetype" },
        };

    private static readonly Int32 MaxLength =
        WellKnownAttributeNames.Keys.Select(x => x.Length).Max();

    public static String? TryGetWellKnownTagName(ICharBuffer builder)
    {
        var buffer = ArrayPool<Char>.Shared.Rent(MaxLength);
        try
        {
            var written = builder.TryCopyTo(buffer);
            if (written != null && WellKnownAttributeNames.TryGetValue(new StringOrMemory(written.Value), out var name))
            {
                return name;
            }

            return null;
        }
        finally
        {
            ArrayPool<Char>.Shared.Return(buffer);
        }
    }
}