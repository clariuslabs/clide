using System;
using System.ComponentModel.Composition;
using Merq;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Clide.Components.Interop
{
	[PartCreationPolicy(CreationPolicy.Shared)]
	internal class VsBooleanSymbolExpressionEvaluatorProvider
	{
		static Guid BooleanSymbolExpressionEvaluatorClsid = new Guid("5DADF1EE-BCBE-46CE-BADF-271992C112A3");
		Lazy<IVsBooleanSymbolExpressionEvaluator> expressionEvaluator;

		[ImportingConstructor]
		public VsBooleanSymbolExpressionEvaluatorProvider([Import(typeof(SVsServiceProvider))] IServiceProvider services, IAsyncManager async)
		{
			expressionEvaluator = new Lazy<IVsBooleanSymbolExpressionEvaluator>(() => async.Run(async () =>
			{
				await async.SwitchToMainThread();

				var registry = services.GetService<SLocalRegistry, ILocalRegistry>();

				// dev14+ should provide the evaluator using the BooleanSymbolExpressionEvaluator clsid
				var value = registry.CreateInstance(BooleanSymbolExpressionEvaluatorClsid) as IVsBooleanSymbolExpressionEvaluator;
				if (value == null)
				{
					// Previous versions of VS provides the service using the VsProjectCapabilityExpressionMatcher interface
					value = registry.CreateInstance(typeof(VsProjectCapabilityExpressionMatcher).GUID) as IVsBooleanSymbolExpressionEvaluator;
				}

				return value;
			}));
		}

		[Export(ContractNames.Interop.IVsBooleanSymbolExpressionEvaluator)]
		public IVsBooleanSymbolExpressionEvaluator ExpressionEvaluator => expressionEvaluator.Value;
	}
}
