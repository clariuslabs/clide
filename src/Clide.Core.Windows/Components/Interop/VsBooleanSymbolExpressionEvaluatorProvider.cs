using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Threading;

namespace Clide.Components.Interop
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    internal class VsBooleanSymbolExpressionEvaluatorProvider
    {
        internal static Guid BooleanSymbolExpressionEvaluatorClsid = new Guid("5DADF1EE-BCBE-46CE-BADF-271992C112A3");

        [Export(ContractNames.Interop.IVsBooleanSymbolExpressionEvaluator)]
        [Export(typeof(JoinableLazy<IVsBooleanSymbolExpressionEvaluator>))]
        readonly JoinableLazy<IVsBooleanSymbolExpressionEvaluator> expressionEvaluator;

        [ImportingConstructor]
        public VsBooleanSymbolExpressionEvaluatorProvider([Import(typeof(SVsServiceProvider))] IServiceProvider services, JoinableTaskContext context)
        {
            expressionEvaluator = new JoinableLazy<IVsBooleanSymbolExpressionEvaluator>(() =>
            {
                var registry = services.GetService<SLocalRegistry>() as ILocalRegistry;
                // dev14+ should provide the evaluator using the BooleanSymbolExpressionEvaluator clsid
                var value = registry?.CreateInstance(BooleanSymbolExpressionEvaluatorClsid) as IVsBooleanSymbolExpressionEvaluator;

                // Previous versions of VS provides the service using the VsProjectCapabilityExpressionMatcher interface
                return value ?? registry?.CreateInstance(typeof(VsProjectCapabilityExpressionMatcher).GUID) as IVsBooleanSymbolExpressionEvaluator;
            }, context?.Factory, executeOnMainThread: true);
        }

        [Export(ContractNames.Interop.IVsBooleanSymbolExpressionEvaluator)]
        public IVsBooleanSymbolExpressionEvaluator ExpressionEvaluator => expressionEvaluator.GetValue();
    }
}
