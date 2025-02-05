namespace AngleSharp.Dom
{
    using AngleSharp.Html.Dom;
    using AngleSharp.Text;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    /// <summary>
    /// Useful methods for node objects.
    /// </summary>
    public static class NodeExtensions
    {
        /// <summary>
        /// Gets the root of the given node, which is the node itself, if it has
        /// no parent, or the root of the parent.
        /// </summary>
        /// <param name="node">The node to get the root of.</param>
        /// <returns>The root node.</returns>
        public static INode GetRoot(this INode node)
        {
            while (node.Parent is not null)
            {
                node = node.Parent;
            }

            return node;
        }

        /// <summary>
        /// Checks if the provided node is an endpoint, i.e., does not host any
        /// other node.
        /// </summary>
        /// <param name="node">The node that is checked.</param>
        /// <returns>True if the node is an endpoint, otherwise false.</returns>
        public static Boolean IsEndPoint(this INode node)
        {
            var type = node.NodeType;
            return type != NodeType.Document &&
                   type != NodeType.DocumentFragment &&
                   type != NodeType.Element;
        }

        /// <summary>
        /// Checks if the provided <see cref="NodeType"/> is an endpoint, i.e., does not host any
        /// other node.
        /// </summary>
        /// <param name="type">The <see cref="NodeType"/> that is checked.</param>
        /// <returns>True if the node is an endpoint, otherwise false.</returns>
        public static Boolean IsEndPoint(this NodeType type)
        {
            return type != NodeType.Document &&
                   type != NodeType.DocumentFragment &&
                   type != NodeType.Element;
        }

        /// <summary>
        /// Checks if the provided node can be inserted into some other node.
        /// This excludes, e.g., documents from being inserted.
        /// </summary>
        /// <param name="node">The node that is checked.</param>
        /// <returns>True if the node is insertable, otherwise false.</returns>
        public static Boolean IsInsertable(this INode node)
        {
            var type = node.NodeType;
            return type == NodeType.Element || type == NodeType.Comment ||
                   type == NodeType.Text || type == NodeType.ProcessingInstruction ||
                   type == NodeType.DocumentFragment || type == NodeType.DocumentType;
        }

        /// <summary>
        /// Gets the hyperreference of the given URL - transforming the given
        /// (relative) URL to an absolute URL if required.
        /// </summary>
        /// <param name="node">The node that spawns the hyper reference.</param>
        /// <param name="url">The given URL.</param>
        /// <returns>The absolute URL.</returns>
        [return: NotNullIfNotNull("url")]
        public static Url? HyperReference(this INode node, String url) => url is null ? null : new Url(node.BaseUrl!, url);

        /// <summary>
        /// Checks if the node is an descendant of the given parent.
        /// </summary>
        /// <param name="node">The descendant node to use.</param>
        /// <param name="parent">The possible parent to use.</param>
        /// <returns>
        /// True if the given parent is actually an ancestor of the node.
        /// </returns>
        public static Boolean IsDescendantOf(this INode node, INode parent)
        {
            while (node.Parent is not null)
            {
                if (Object.ReferenceEquals(node.Parent, parent))
                {
                    return true;
                }

                node = node.Parent;
            }

            return false;
        }

        /// <summary>
        /// Gets the descendant nodes of the provided parent, in tree order.
        /// </summary>
        /// <param name="parent">The parent of the descendants.</param>
        /// <returns>An iterator over all descendants.</returns>
        public static IEnumerable<INode> GetDescendants(this INode parent) => GetDescendantsAndSelf(parent).Skip(1);

        /// <summary>
        /// Gets the descendant nodes and itself of the provided parent, in tree order.
        /// </summary>
        /// <param name="parent">The parent of the descendants.</param>
        /// <returns>An iterator over all descendants and itself.</returns>
        public static IEnumerable<INode> GetDescendantsAndSelf(this INode parent)
        {
            return GetDescendantsAndSelf<Object?>(parent, new Stack<INode>(), null, null);
        }

        /// <summary>
        /// Gets the descendant nodes and itself of the provided parent, in tree order.
        /// </summary>
        /// <param name="parent">The parent of the descendants.</param>
        /// <param name="stack">Stack instance to be used (allows reuse).</param>
        /// <param name="filter">Optional filter to run against items.</param>
        /// <param name="state">Optional state to help with filtering.</param>
        /// <returns>An iterator over all descendants and itself.</returns>
        internal static IEnumerable<INode> GetDescendantsAndSelf<TState>(this INode parent, Stack<INode> stack, Func<INode, TState?, Boolean>? filter = null, TState? state = default)
        {
            stack.Push(parent);

            while (stack.Count > 0)
            {
                var next = stack.Pop();

                if (filter == null || filter(next, state))
                {
                    yield return next;
                }

                var childNodes = next.ChildNodes;

                // we only have one implementation
                if (childNodes is NodeList nodeList)
                {
                    var length = nodeList.Length;
                    while (length > 0)
                    {
                        stack.Push(nodeList[--length]);
                    }
                }
                else
                {
                    // unlikely virtual dispatch
                    var length = childNodes.Length;
                    while (length > 0)
                    {
                        stack.Push(childNodes[--length]);
                    }
                }
            }
        }

        /// <summary>
        /// Checks if the node is an inclusive descendant of the given parent.
        /// </summary>
        /// <param name="node">The descendant node to use.</param>
        /// <param name="parent">The possible parent to use.</param>
        /// <returns>
        /// True if the given parent is actually an inclusive ancestor of the
        /// provided node.
        /// </returns>
        public static Boolean IsInclusiveDescendantOf(this INode node, INode parent) => node == parent || node.IsDescendantOf(parent);

        /// <summary>
        /// Checks if the parent is an ancestor of the given node.
        /// </summary>
        /// <param name="parent">The possible parent to use.</param>
        /// <param name="node">The node to check for being descendant.</param>
        /// <returns>
        /// True if the given parent is actually an ancestor of the node.
        /// </returns>
        public static Boolean IsAncestorOf(this INode parent, INode node) => node.IsDescendantOf(parent);

        /// <summary>
        /// Gets the ancestor nodes of the provided node, in tree order.
        /// </summary>
        /// <param name="node">The child of the ancestors.</param>
        /// <returns>An iterator over all ancestors.</returns>
        public static IEnumerable<INode> GetAncestors(this INode node)
        {
            while ((node = node!.Parent!) is not null)
            {
                yield return node;
            }
        }

        /// <summary>
        /// Gets the inclusive ancestor nodes of the provided node, in tree
        /// order.
        /// </summary>
        /// <param name="node">The child of the ancestors.</param>
        /// <returns>
        /// An iterator over all ancestors including the given node.
        /// </returns>
        public static IEnumerable<INode> GetInclusiveAncestors(this INode node)
        {
            do
            {
                yield return node;
            }
            while ((node = node!.Parent!) is not null);
        }

        /// <summary>
        /// Checks if the parent is an inclusive ancestor of the given node.
        /// </summary>
        /// <param name="parent">The possible parent to use.</param>
        /// <param name="node">The node to check for being descendant.</param>
        /// <returns>
        /// True if the given parent is actually an inclusive ancestor of the
        /// provided node.
        /// </returns>
        public static Boolean IsInclusiveAncestorOf(this INode parent, INode node) => node == parent || node.IsDescendantOf(parent);

        /// <summary>
        /// Gets the first ancestor node that is of the specified type.
        /// </summary>
        /// <param name="node">The child of the potential ancestor.</param>
        /// <returns>The specified ancestor or its default value.</returns>
        public static T? GetAncestor<T>(this INode node)
            where T : INode
        {
            while ((node = node!.Parent!) is not null)
            {
                if (node is T t)
                {
                    return t;
                }
            }

            return default;
        }

        /// <summary>
        /// Checks if any parent is an HTML datalist element..
        /// </summary>
        /// <param name="child">The node to use as starting point.</param>
        /// <returns>
        /// True if a datalist element is among the ancestors, otherwise false.
        /// </returns>
        public static Boolean HasDataListAncestor(this INode child) => child.Ancestors<IHtmlDataListElement>().Any();

        /// <summary>
        /// Checks if the current node is a sibling of the specified element.
        /// </summary>
        /// <param name="node">The maybe sibling.</param>
        /// <param name="element">
        /// The node to check for having the same parent.
        /// </param>
        /// <returns>
        /// True if the parent is actually non-null and actually the same.
        /// </returns>
        public static Boolean IsSiblingOf(this INode node, INode element) => node?.Parent == element.Parent;

        /// <summary>
        /// Gets the index of the provided node in the parent's collection.
        /// </summary>
        /// <param name="node">The node which needs to know its index.</param>
        /// <returns>
        /// The index of the node or -1 if the node is not a child of a parent.
        /// </returns>
        public static Int32 Index(this INode node) => node.Parent!.IndexOf(node);

        /// <summary>
        /// Finds the index of the given node of the provided parent node.
        /// </summary>
        /// <param name="parent">The parent of the given node.</param>
        /// <param name="node">The node which needs to know its index.</param>
        /// <returns>
        /// The node's index or -1 if the node is not a child of the parent.
        /// </returns>
        public static Int32 IndexOf(this INode parent, INode node)
        {
            var i = 0;

            if (parent is not null)
            {
                foreach (var child in parent.ChildNodes)
                {
                    if (Object.ReferenceEquals(child, node))
                    {
                        return i;
                    }

                    i++;
                }
            }

            return -1;
        }

        /// <summary>
        /// Checks if the context node is before the provided node.
        /// </summary>
        /// <param name="before">The context node.</param>
        /// <param name="after">The provided ref node.</param>
        /// <returns>
        /// True if the context node is preceding the ref node in tree order.
        /// </returns>
        public static Boolean IsPreceding(this INode before, INode after)
        {
            var beforeNodes = new Queue<INode>(before.GetInclusiveAncestors());
            var afterNodes = new Queue<INode>(after.GetInclusiveAncestors());
            var skew = afterNodes.Count - beforeNodes.Count;

            if (skew != 0)
            {
                while (beforeNodes.Count > afterNodes.Count)
                {
                    beforeNodes.Dequeue();
                }

                while (afterNodes.Count > beforeNodes.Count)
                {
                    afterNodes.Dequeue();
                }

                if (IsCurrentlySame(afterNodes, beforeNodes))
                {
                    return skew > 0;
                }
            }

            while (beforeNodes.Count > 0)
            {
                before = beforeNodes.Dequeue();
                after = afterNodes.Dequeue();

                if (IsCurrentlySame(afterNodes, beforeNodes))
                {
                    return before.Index() < after.Index();
                }
            }

            return false;
        }

        /// <summary>
        /// Checks if the context node is after the provided node.
        /// </summary>
        /// <param name="after">The context node.</param>
        /// <param name="before">The provided ref node.</param>
        /// <returns>
        /// True if the context node is following the ref node in tree order.
        /// </returns>
        public static Boolean IsFollowing(this INode after, INode before) => before.IsPreceding(after);

        /// <summary>
        /// Gets the associated host object, if any. This is mostly interesting
        /// for the HTML5 template tag.
        /// </summary>
        /// <param name="node">The node that probably has an host object</param>
        /// <returns>The host object or null.</returns>
        public static INode? GetAssociatedHost(this INode node)
        {
            if (node is IDocumentFragment)
            {
                return node.Owner?.All.OfType<IHtmlTemplateElement>().FirstOrDefault(m => m.Content == node);
            }

            return null;
        }

        /// <summary>
        /// Checks for an inclusive ancestor relationship or if the host (if
        /// any) has such a relationship.
        /// </summary>
        /// <param name="parent">The possible parent to use.</param>
        /// <param name="node">The node to check for being descendant.</param>
        /// <returns>
        /// True if the given parent is actually an inclusive ancestor
        /// (including the host) of the provided node.
        /// </returns>
        public static Boolean IsHostIncludingInclusiveAncestor(this INode parent, INode node)
        {
            if (!parent.IsInclusiveAncestorOf(node))
            {
                var host = node.GetRoot().GetAssociatedHost();

                if (host is not null)
                {
                    return parent.IsInclusiveAncestorOf(host);
                }

                return false;
            }

            return true;
        }

        /// <summary>
        /// Ensures the validity for inserting the given node at parent before
        /// the provided child. Throws an error is the insertation is invalid.
        /// </summary>
        /// <param name="parent">The origin that will be mutated.</param>
        /// <param name="node">The node to be inserted.</param>
        /// <param name="child">The reference node of the insertation.</param>
        public static void EnsurePreInsertionValidity(this INode parent, INode node, INode? child)
        {
            if (parent.IsEndPoint() || node.IsHostIncludingInclusiveAncestor(parent))
            {
                throw new DomException(DomError.HierarchyRequest);
            }

            if (child is not null && child.Parent != parent)
            {
                throw new DomException(DomError.NotFound);
            }

            if (node is IElement == false && node is ICharacterData == false && node is IDocumentType == false && node is IDocumentFragment == false)
            {
                throw new DomException(DomError.HierarchyRequest);
            }

            if (parent is IDocument document)
            {
                var forbidden = false;

                switch (node.NodeType)
                {
                    case NodeType.Element:
                        forbidden = document.DocumentElement is not null || child is IDocumentType || child.IsFollowedByDoctype();
                        break;
                    case NodeType.DocumentFragment:
                        var elements = node.GetElementCount();
                        forbidden = elements > 1 || node.HasTextNodes() || (elements == 1 && document.DocumentElement is not null) || child is IDocumentType || child.IsFollowedByDoctype();
                        break;
                    case NodeType.DocumentType:
                        forbidden = document.Doctype is not null || (child is not null && child.IsPrecededByElement()) || (child is null && document.DocumentElement is not null);
                        break;
                    case NodeType.Text:
                        forbidden = true;
                        break;
                }

                if (forbidden)
                {
                    throw new DomException(DomError.HierarchyRequest);
                }
            }
            else if (node is IDocumentType)
            {
                throw new DomException(DomError.HierarchyRequest);
            }
        }

        /// <summary>
        /// Pre-inserts the given node at the parent before the provided child.
        /// </summary>
        /// <param name="parent">The origin that will be mutated.</param>
        /// <param name="node">The node to be inserted.</param>
        /// <param name="child">The reference node of the insertation.</param>
        /// <returns>The inserted node, which is node.</returns>
        public static INode PreInsert(this INode parent, INode node, INode? child)
        {
            var newNode = (Node)node;

            if (parent is Node parentNode)
            {
                parent.EnsurePreInsertionValidity(node, child);
                var referenceChild = child as Node;

                if (referenceChild == node)
                {
                    referenceChild = newNode.NextSibling;
                }

                var document = parent.Owner ?? parent as IDocument;
                document!.AdoptNode(node);
                parentNode.InsertBefore(newNode, referenceChild, false);
                return node;
            }

            throw new DomException(DomError.NotSupported);
        }

        /// <summary>
        /// Pre-removes the given child of the parent.
        /// </summary>
        /// <param name="parent">The origin that will be mutated.</param>
        /// <param name="child">The node that will be removed.</param>
        /// <returns>The removed node, which is child.</returns>
        public static INode PreRemove(this INode parent, INode child)
        {
            if (parent is Node parentNode)
            {
                if (child is null || child.Parent != parent)
                {
                    throw new DomException(DomError.NotFound);
                }

                parentNode.RemoveChild((Node)child, false);
                return child;
            }

            throw new DomException(DomError.NotSupported);
        }

        /// <summary>
        /// Checks if the node has any text node children.
        /// </summary>
        /// <param name="node">The parent of the potential text nodes.</param>
        /// <returns>
        /// True if the node has any text nodes, otherwise false.
        /// </returns>
        public static Boolean HasTextNodes(this INode node) => node.ChildNodes.OfType<IText>().Any();

        /// <summary>
        /// Checks if the given child is followed by a document type.
        /// </summary>
        /// <param name="child">The child that precedes the doctype.</param>
        /// <returns>
        /// True if a doctype node is following the child, otherwise false.
        /// </returns>
        public static Boolean IsFollowedByDoctype(this INode? child)
        {
            if (child is not null)
            {
                var before = true;

                foreach (var node in child.Parent!.ChildNodes)
                {
                    if (before)
                    {
                        before = node != child;
                    }
                    else if (node.NodeType == NodeType.DocumentType)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Checks if the given child is preceded by an element node.
        /// </summary>
        /// <param name="child">The child that follows any element.</param>
        /// <returns>
        /// True if an element node is preceded the child, otherwise false.
        /// </returns>
        public static Boolean IsPrecededByElement(this INode child)
        {
            foreach (var node in child.Parent!.ChildNodes)
            {
                if (node == child)
                {
                    break;
                }
                else if (node.NodeType == NodeType.Element)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets the element count of the given node.
        /// </summary>
        /// <param name="parent">The parent of potential element nodes.</param>
        /// <returns>The number of element nodes in the parent.</returns>
        public static Int32 GetElementCount(this INode parent)
        {
            var count = 0;

            foreach (var node in parent.ChildNodes)
            {
                if (node.NodeType == NodeType.Element)
                {
                    count++;
                }
            }

            return count;
        }

        /// <summary>
        /// Tries to find a direct child of a certain type.
        /// </summary>
        /// <typeparam name="TNode">The node type to find.</typeparam>
        /// <param name="parent">The parent that contains the elements.</param>
        /// <returns>The instance or null.</returns>
        public static TNode? FindChild<TNode>(this INode parent)
            where TNode : class, INode
        {
            if (parent is not null)
            {
                for (var i = 0; i < parent.ChildNodes.Length; i++)
                {

                    if (parent.ChildNodes[i] is TNode child)
                    {
                        return child;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Tries to find a descendant of a certain type.
        /// </summary>
        /// <typeparam name="TNode">The node type to find.</typeparam>
        /// <param name="parent">The parent that contains the elements.</param>
        /// <param name="maxDepth">The maximum depth to allow for searching. A value of 0 is equivalent to FindChild.</param>
        /// <returns>The instance or null.</returns>
        public static TNode? FindDescendant<TNode>(this INode parent, Int32 maxDepth = 1024)
            where TNode : class, INode
        {
            if (parent is not null && maxDepth > -1)
            {
                for (var i = 0; i < parent.ChildNodes.Length; i++)
                {
                    var node = parent.ChildNodes[i];
                    var child = node as TNode ?? node.FindDescendant<TNode>(maxDepth - 1);

                    if (child is not null)
                    {
                        return child;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the assigned slot given by the shadow root and the slot name.
        /// </summary>
        /// <param name="root">The shadow tree hosting the slots.</param>
        /// <param name="name">The name of the slot to target.</param>
        /// <returns>The slot or default slot, if any.</returns>
        public static IElement? GetAssignedSlot(this IShadowRoot root, String? name) => root.GetDescendants().OfType<IHtmlSlotElement>().FirstOrDefault(m => m.Name.Is(name));

        /// <summary>
        /// Gets the content text of the given DOM node.
        /// </summary>
        /// <param name="node">The node to stringify.</param>
        /// <returns>The text of the node and its children.</returns>
        public static String Text(this INode node)
        {
            if (node is null)
            {
                throw new ArgumentNullException(nameof(node));
            }

            return node.TextContent;
        }

        /// <summary>
        /// Sets the text content of the given elements.
        /// </summary>
        /// <typeparam name="T">The type of collection.</typeparam>
        /// <param name="nodes">The collection.</param>
        /// <param name="text">The text that should be set.</param>
        /// <returns>The collection itself.</returns>
        public static T Text<T>(this T nodes, String text)
            where T : IEnumerable<INode>
        {
            if (nodes is null)
            {
                throw new ArgumentNullException(nameof(nodes));
            }

            foreach (var element in nodes)
            {
                element.TextContent = text;
            }

            return nodes;
        }

        /// <summary>
        /// Gets the index of the given item in the list of nodes.
        /// </summary>
        /// <param name="nodes">The source list of nodes.</param>
        /// <param name="item">The item to search for.</param>
        /// <returns>The index of the item or -1 if not found.</returns>
        public static Int32 Index(this IEnumerable<INode> nodes, INode item)
        {
            if (nodes is null)
            {
                throw new ArgumentNullException(nameof(nodes));
            }

            if (item is not null)
            {
                var i = 0;

                foreach (var node in nodes)
                {
                    if (Object.ReferenceEquals(node, item))
                    {
                        return i;
                    }

                    i++;
                }
            }

            return -1;
        }

        private static Boolean IsCurrentlySame(Queue<INode> after, Queue<INode> before) => after.Count > 0 && before.Count > 0 && after.Peek() == before.Peek();
    }
}
