using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
#if PORTABLE
using ExcludeFromCodeCoverage=System.Diagnostics.DebuggerNonUserCodeAttribute;
#endif

[assembly:InternalsVisibleTo("PRI.PrereleaseAttributes.Analyzer.Test")]
namespace PRI.PrereleaseAttributes.Analyzer
{
	/// <summary>
	/// A class that wraps a SyntaxNode in order to provide a consistent API to get at
	/// <seealso cref="Type"/>, <seealso cref="Identifier"/>,
	/// <seealso cref="DescendantNodes"/>, <seealso cref="AttributeLists"/>,
	/// <seealso cref="Parent"/>, and <seealso cref="Ancestors"/>.
	/// Seems like there would be a type in the hierarchy for this, but no.
	/// </summary>
	[ExcludeFromCodeCoverage]
	internal class DeclarationSyntaxFacade
	{
		private readonly SemanticModel _semanticModel;
		private readonly SyntaxNode _syntaxNode;

		public static implicit operator SyntaxNode(DeclarationSyntaxFacade facade)
		{
			return facade._syntaxNode;
		}

		public override string ToString()
		{
			return _syntaxNode.ToString();
		}

		public DeclarationSyntaxFacade(SemanticModel semanticModel, [NotNull]MemberDeclarationSyntax memberDeclarationSyntax)
		{
#if false
			if (memberDeclarationSyntax == null)
			{
				throw new ArgumentNullException(nameof(memberDeclarationSyntax));
			}
#endif
			_semanticModel = semanticModel;
			_syntaxNode = memberDeclarationSyntax;

			Init(memberDeclarationSyntax);
		}
		public DeclarationSyntaxFacade(SemanticModel semanticModel, LocalDeclarationStatementSyntax localDeclarationStatementSyntax)
		{
#if false // only null on compilation unit node?  We don't support that
			if (localDeclarationStatementSyntax == null)
			{
				throw new ArgumentNullException(nameof(localDeclarationStatementSyntax));
			}
#endif
			_semanticModel = semanticModel;
			_syntaxNode = localDeclarationStatementSyntax;
			Init(localDeclarationStatementSyntax);
		}

		private DeclarationSyntaxFacade(SemanticModel semanticModel, SyntaxNode syntaxNode)
		{
#if false // only null on compilation unit node?  We don't support that
			if (syntaxNode == null)
			{
				throw new ArgumentNullException(nameof(syntaxNode));
			}
#endif
			_semanticModel = semanticModel;
			_syntaxNode = syntaxNode;
#if LOCALS
			var local = syntaxNode as LocalDeclarationStatementSyntax;
			if (local != null)
			{
				Init(local);
			}
			else
			{
#endif // LOCALS
				var member = syntaxNode as MemberDeclarationSyntax;
				if (member != null)
				{
					Init(member);
				}
			var unit = syntaxNode as CompilationUnitSyntax;
			if (unit != null)
			{
				Init(unit);
			}
#if LOCALS
			}
#endif // LOCALS
		}

		private void Init(CompilationUnitSyntax compilationUnitSyntax)
		{
			AttributeLists = compilationUnitSyntax.AttributeLists;
		}


		private void Init(MemberDeclarationSyntax memberDeclarationSyntax)
		{
			var fieldDeclaration = memberDeclarationSyntax as FieldDeclarationSyntax;
			if (fieldDeclaration != null)
			{
				var variableDeclarationSyntax = fieldDeclaration.DescendantNodes().OfType<VariableDeclarationSyntax>().Single();
				var variable = variableDeclarationSyntax.Variables.Single();
				AttributeLists = fieldDeclaration.AttributeLists;
				Identifier = variable.Identifier;
				Type = _semanticModel.GetTypeInfo(variableDeclarationSyntax.Type).Type;

				return;
			}
			var propertyDeclaration = memberDeclarationSyntax as PropertyDeclarationSyntax;
			if (propertyDeclaration != null)
			{
				AttributeLists = propertyDeclaration.AttributeLists;
				Identifier = propertyDeclaration.Identifier;
				Type = _semanticModel.GetTypeInfo(propertyDeclaration.Type).Type;

				return;
			}
			var indexerDeclaration = memberDeclarationSyntax as IndexerDeclarationSyntax;
			if (indexerDeclaration != null)
			{
				AttributeLists = indexerDeclaration.AttributeLists;
				Identifier = indexerDeclaration.ThisKeyword;
				Type = _semanticModel.GetTypeInfo(indexerDeclaration.Type).Type;

				return;
			}
			var methodDeclaration = memberDeclarationSyntax as MethodDeclarationSyntax;
			if (methodDeclaration != null)
			{
				AttributeLists = methodDeclaration.AttributeLists;
				Identifier = methodDeclaration.Identifier;
				Type = _semanticModel.GetTypeInfo(methodDeclaration.ReturnType).Type;

				return;
			}
			var typeDeclaration = memberDeclarationSyntax as BaseTypeDeclarationSyntax;
			if (typeDeclaration != null)
			{
				AttributeLists = typeDeclaration.AttributeLists;
				Identifier = typeDeclaration.Identifier;
				Type = _semanticModel.GetTypeInfo(typeDeclaration).Type;

				return;
			}
			var enumDeclaration = memberDeclarationSyntax as EnumMemberDeclarationSyntax;
			if (enumDeclaration != null)
			{
				AttributeLists = enumDeclaration.AttributeLists;
				Identifier = enumDeclaration.Identifier;
				Type = _semanticModel.GetTypeInfo(enumDeclaration).Type;

				return;
			}

			var namespaceDeclaration = memberDeclarationSyntax as NamespaceDeclarationSyntax;
			if (namespaceDeclaration != null)
			{
				AttributeLists = new SyntaxList<AttributeListSyntax>();
				Identifier = namespaceDeclaration.NamespaceKeyword;
				Type = _semanticModel.GetTypeInfo(namespaceDeclaration).Type;

				return;
			}
			throw new InvalidOperationException($"Unsupported type {nameof(memberDeclarationSyntax)}");
		}

#if false
		// TODO: Remove? DeclarationSyntaxWrapper(SemanticModel semanticModel, VariableDeclarationSyntax variableDeclarationSyntax)
		public DeclarationSyntaxWrapper(SemanticModel semanticModel, VariableDeclarationSyntax variableDeclarationSyntax)
		{
			if (variableDeclarationSyntax == null)
			{
				throw new ArgumentNullException(nameof(variableDeclarationSyntax));
			}
			_semanticModel = semanticModel;
			_syntaxNode = variableDeclarationSyntax;
			Init(variableDeclarationSyntax);
		}

		private void Init(VariableDeclarationSyntax variableDeclarationSyntax)
		{
			Type = _semanticModel.GetTypeInfo(variableDeclarationSyntax.Type).Type;
			var variable = variableDeclarationSyntax.Variables.Single();
			Identifier = variable.Identifier;
		}
#endif

		private void Init(LocalDeclarationStatementSyntax localDeclarationStatementSyntax)
		{
			var variableDeclarationSyntax = localDeclarationStatementSyntax.Declaration;
			Type = _semanticModel.GetTypeInfo(variableDeclarationSyntax.Type).Type;
			var variable = variableDeclarationSyntax.Variables.Single();
			Identifier = variable.Identifier;
		}

		public ITypeSymbol Type { get; private set; }

		public SyntaxToken Identifier { get; private set; }

		public IEnumerable<SyntaxNode> DescendantNodes()
		{
			return _syntaxNode.DescendantNodes();
		}
		public SyntaxList<AttributeListSyntax> AttributeLists { get; private set; }
		public DeclarationSyntaxFacade Parent => new DeclarationSyntaxFacade(_semanticModel, _syntaxNode.Parent);

		public IEnumerable<SyntaxNode> Ancestors()
		{
			return _syntaxNode.Ancestors();
		}
	}
}