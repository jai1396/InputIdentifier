using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.CodeAnalysis.Shared.Utilities;

namespace InputIdentifier
{
    class SemanticAnalyzer
    {
        #region mscorlib
        private MetadataReference mscorlib;

        private MetadataReference Mscorlib
        {
            get
            {
                if (mscorlib == null)
                {
                    mscorlib = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
                }

                return mscorlib;
            }
        }
        #endregion
        
        public void CompareSymbols()
        {
            //            var tree = SyntaxFactory.ParseSyntaxTree(@"
            //using System;
            //class C
            //{
            //}
            //class Program
            //{
            //    public static void Main()
            //    {
            //        var c = new C(); 
            //        c+=x;
            //        Console.WriteLine(c.ToString());
            //    }
            //}");

            var tree = SyntaxFactory.ParseSyntaxTree(@"
class Program
{
    public static void Main()
    {
        //int text = 5;
        MatchCollection mc = Regex.Matches(text, expr);

    }
}");
            var compilation = CSharpCompilation.Create("MyCompilation",
                syntaxTrees: new[] { tree }, references: new[] { Mscorlib });
            var model = compilation.GetSemanticModel(tree);

            IdentifierWalker iw = new IdentifierWalker();
            iw.model = model;
            iw.compilation = compilation;
            iw.Visit(tree.GetRoot());

            Console.WriteLine();
            Console.WriteLine(iw.sb.ToString());

            Console.WriteLine("\nUndeclared:\n");
            foreach (var s in iw.undeclaredIdentifiers)
            {
                Console.WriteLine(s);
            }
            
        }

        class IdentifierWalker : CSharpSyntaxWalker
        {
            public CSharpCompilation compilation { get; set; }
            public SemanticModel model { get; set; }

            List<SyntaxKind> disablers = new List<SyntaxKind>() { SyntaxKind.DotToken };
            List<string> excludedIdentifiers = new List<string>() { "var", "Program", "Main" };
            List<string> declaredIdentifiers = new List<string>();
            public List<string> undeclaredIdentifiers = new List<string>();

            public IdentifierWalker() : base(SyntaxWalkerDepth.Token)
            {

            }

            public override void VisitToken(SyntaxToken token)
            {
                //Console.WriteLine(token.ToFullString());
                if (token.Kind() == SyntaxKind.IdentifierToken)
                {
                    VisitIdentifierToken(token);
                }
                base.VisitToken(token);
            }

            public StringBuilder sb = new StringBuilder();

            private void VisitIdentifierToken(SyntaxToken token)
            {
                sb.AppendLine(token.Text);

                if (excludedIdentifiers.Contains(token.Text))
                {
                    return;
                }

                var parent = token.Parent;
                if (parent.Kind() == SyntaxKind.IdentifierName)
                {
                    VisitIdNameAsParentOfToken((IdentifierNameSyntax)parent);
                }
                else
                {
                    VisitOtherParentOfToken(token);
                }
            }

            public void VisitIdNameAsParentOfToken(IdentifierNameSyntax node)
            {
                if (IdNodePrecededByDisabler(node))
                {
                    return;
                }

                var text = node.Identifier.Text;
                if (!declaredIdentifiers.Contains(text))
                {
                    if (!undeclaredIdentifiers.Contains(text))
                    {
                        undeclaredIdentifiers.Add(text);
                    }
                    
                }

                var type = model.GetTypeInfo(node).Type;

                Console.WriteLine(node.Identifier.Text + ": " + type);
            }

            public void VisitOtherParentOfToken(SyntaxToken token)
            {
                if (IdTokenPrecededByDisabler(token))
                {
                    return;
                }

                if (!declaredIdentifiers.Contains(token.Text))
                {
                    declaredIdentifiers.Add(token.Text);
                }

                var type = model.GetTypeInfo(token.Parent).Type;

                Console.WriteLine(token.Text + ": " + type);

            }

            public bool IdNodePrecededByDisabler(IdentifierNameSyntax node)
            {
                var spanstart = node.SpanStart;
                var parent = node.Parent;
                var precedingToken = parent.ChildTokens().Where(t => t.FullSpan.End == spanstart).SingleOrDefault();
                if (precedingToken != null)
                {
                    if (disablers.Contains(precedingToken.Kind()))
                    {
                        return true;
                    }
                }
                return false;
            }

            public bool IdTokenPrecededByDisabler(SyntaxToken token)
            {
                var spanstart = token.SpanStart;
                var parent = token.Parent;
                var precedingToken = parent.ChildTokens().Where(t => t.FullSpan.End == spanstart).SingleOrDefault();
                if (precedingToken != null)
                {
                    if (disablers.Contains(precedingToken.Kind()))
                    {
                        return true;
                    }
                }
                return false;
            }

            //public override void VisitVariableDeclaration(VariableDeclarationSyntax node)
            //{
            //    Console.WriteLine();
            //    Console.WriteLine();
            //    Console.WriteLine(node.ToFullString());
            //    var symbol = model.GetDeclaredSymbol(node);
            //    if (symbol!=null)
            //    {
            //        Console.WriteLine(symbol.Name);
            //    }
            //    node.
            //    base.VisitVariableDeclaration(node);
            //}

            //public override void VisitVariableDeclarator(VariableDeclaratorSyntax variableDeclarator)
            //{
            //    Console.WriteLine();
            //    Console.WriteLine();
            //    Console.WriteLine(variableDeclarator.ToFullString());
            //    var type = ((ILocalSymbol)model.GetDeclaredSymbol(variableDeclarator)).Type;
            //    Console.WriteLine(type.Name);
            //    model.LookupSymbols(variableDeclarator.S)
            //    base.VisitVariableDeclarator(variableDeclarator);
            //}
        }
    }
}
