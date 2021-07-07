// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using Cilsil.Sil;
using Cilsil.Sil.Expressions;
using Cilsil.Sil.Instructions;
using Cilsil.Sil.Types;
using Cilsil.Utils;
using Mono.Cecil.Cil;
using System.Collections.Generic;

namespace Cilsil.Cil.Parsers
{
    /// <summary>
    /// Represents program expressions.
    /// </summary>
    internal abstract class ExceptionParser : InstructionParser
    {
        /// <summary>
        /// Abstract method for creating exception handling block content nodes.
        /// </summary>
        /// <param name="state">Current program state.</param>
        /// <returns><c>true</c> if the exception block is translated successfully, <c>false</c> 
        /// otherwise.</returns>
        public abstract bool TranslateBlock(ProgramState state);

        /// <summary>
        /// Abstract method for creating exception handling block door node.
        /// </summary>
        /// <param name="state">Current program state.</param>
        public static CfgNode CreateExceptionDoor(ProgramState state)
        {
            /* Catch/Finally block exception door node looks like this
            
            node1 preds: succs:2 3 exn: Instructions
            n$47=*&amp;return:void;
            *&amp;return:void=null;
            n$48=_fun___unwrap_exception(n$47:void);*/

            // Create door node.
            var doorNode = new StatementNode(state.CurrentLocation,
                                             StatementNode.StatementNodeKind.ExceptionHandler,
                                             state.ProcDesc);

            // Create load returned variable expression.  
            // Example: n$47=*&amp;return:void;
            var returnedExpression = new LvarExpression(
                                        new LocalVariable(Identifier.ReturnIdentifier,
                                                          state.Method));
            var returnType = Typ.FromTypeReference(state.Method.ReturnType);
            var identifier = state.GetIdentifier(Identifier.IdentKind.Normal);
            doorNode.Instructions.Add(new Load(identifier,
                                               returnedExpression,
                                               returnType,
                                               state.CurrentLocation));
            
            // Create setting loaded returned variable as null expression.
            // Example: *&amp;return:void=null;
            var storeValueIntoVariable = new Store(returnedExpression,
                                                   new ConstExpression(new IntRepresentation(0, false, true)),
                                                   returnType,
                                                   state.CurrentLocation);
            doorNode.Instructions.Add(storeValueIntoVariable);

            // Construct an instruction to unwrap exception from returned variable.
            // Example: n${i}=_fun___unwrap_exception(n${i-1}:last var type*)
            var exceptionExpression = new VarExpression(identifier);
            var exceptionCall = ExceptionParser.CreateUnwrapExceptionCall(state,
                                                          exceptionExpression,
                                                          returnType);
            doorNode.Instructions.Add(exceptionCall);
            state.Cfg.RegisterNode(doorNode);

            return doorNode;
        }

        /// <summary>
        /// Creates a unwrap exception call invoked by exception block door node.
        /// </summary>
        /// <param name="state">Current program state.</param>
        /// <param name="exceptionExpression">The exception expression.</param>
        /// <param name="exceptionExpressionType">The type of excepted expression.</param>
        /// <returns>The unwrap exception Call SIL instruction.</returns>
        private static Call CreateUnwrapExceptionCall(ProgramState state,
                                               Expression exceptionExpression,
                                               Typ exceptionExpressionType)
        {
            var callArgs = new List<Call.CallArg>();
            var returnType = state.Method.ReturnType;

            var unwrapExceptionExpression = new ConstExpression(ProcedureName.BuiltIn__unwrap_exception);
            
            callArgs.Add(new Call.CallArg(exceptionExpression, exceptionExpressionType));
            var callFlags = new Call.CallFlags(false, false, false);
            var returnVariable = state.GetIdentifier(Identifier.IdentKind.Normal);

            if (returnType != state.Method.Module.TypeSystem.Object)
            {
                state.PushExpr(new VarExpression(returnVariable), Typ.FromTypeReference(returnType));
            }
            return new Call(returnId: returnVariable,
                            returnType: Typ.FromTypeReference(returnType),
                            functionExpression: unwrapExceptionExpression,
                            args: callArgs,
                            flags: callFlags,
                            location: state.CurrentLocation);
        }

        /// <summary>
        /// Creates atrium node for catch/finally block.
        /// </summary>
        /// <param name="state">Current program state.</param>
        /// <param name="catchVariable">Created catch variable.</param>
        /// <returns>The created atrium node.</returns>
        protected virtual CfgNode CreateAttriumNode(ProgramState state, LvarExpression catchVariable)
        {
            /* Load caught exception variable. 
            For example in catch block:
            
            node 4: Preds:2 Succs:6 EXN: 
            n$25=*&CatchVar65:java.lang.Object*;
            *&e:java.lang.Object*=n$25;
            
            In finally block:
            node 26: Preds:29, 35, 41, 43, 45 Succs:27 EXN: 46
            n$27=*&CatchVar77:java.lang.Object*;
            *&$bcvar6:java.lang.Object*=n$27;
            */

            var atriumNode = new StatementNode(location: state.CurrentLocation,
                                            kind: StatementNode.StatementNodeKind.ExceptionHandler,
                                            proc: state.ProcDesc);
            var fieldType = new Tstruct("System.Object");
            var fieldIdentifier = state.GetIdentifier(Identifier.IdentKind.Normal);
            atriumNode.Instructions.Add(new Load(fieldIdentifier,
                                                  catchVariable,
                                                  fieldType,
                                                  state.CurrentLocation));

            return atriumNode;
        }
    }
}