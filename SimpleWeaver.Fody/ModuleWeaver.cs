using Fody;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;

#region code to be present in the target assembly

internal static class CacheManager
{
    internal static object? GetCacheValue(
        string declaringType,
        string methodName,
        params object[] parameters)
    {
        throw new NotImplementedException();
    }

    internal static void SetCacheValue(object value,
        string declaringType, string methodName,
        params object[] parameters)
    {
        throw new NotImplementedException();
    }
    
    internal static void CaptureMethodResult(object value)
    {
        throw new NotImplementedException();
    }
}

#endregion

#region ModuleWeaver

public static class ModuleWeaverExtensions
{
    public static void InsertInstructions(this ILProcessor processor,Instruction targetInstruction,IReadOnlyCollection<Instruction> instructions)
    {
        foreach (var instruction in instructions)
        {
            processor.InsertBefore(targetInstruction, instruction);
        }
    }
}

public class ModuleWeaver :
    BaseModuleWeaver
{
    private string CacheAttributeName = "CacheAttribute";
    #region Execute

    public override void Execute()
    {
        var methods = ModuleDefinition.Types
            .SelectMany(t => t.Methods)
            .Where(m => m.CustomAttributes.Any(attr => attr.AttributeType.Name == CacheAttributeName));

        foreach (var method in methods)
        {
            // Skip methods without a body
            if (!method.HasBody)
            {
                WriteError($"{method.DeclaringType.Name}.{method.Name} is empty.");
            }

            // Skip void methods
            if (method.ReturnType.FullName == typeof(void).FullName)
            {
                WriteError($"{method.DeclaringType.Name}.{method.Name} return void.");
            }

            WriteMessage( $"InjectCache in {method.Name}",MessageImportance.High);
            InjectCache(method);
        }
    }

    #endregion

    #region GetAssembliesForScanning

    public override IEnumerable<string> GetAssembliesForScanning()
    {
        yield return "netstandard";
        yield return "mscorlib";
    }

    #endregion

    private void InjectCache(MethodDefinition method)
    {
        var processor = method.Body.GetILProcessor();
        var instructions = method.Body.Instructions;

        var returnInstructions = instructions.Where(instr => instr.OpCode == OpCodes.Ret).ToList();

        foreach (var returnInstruction in returnInstructions)
        {
            processor.InsertInstructions(returnInstruction, SetCacheValue(method));
        }

        var firstInstruction = instructions.First();
        processor.InsertInstructions(firstInstruction, ReturnGetValueCacheIfAny(method));
    }
    
    private IReadOnlyCollection<Instruction> ReturnGetValueCacheIfAny(MethodDefinition method)
    {
        
        var instructions = new List<Instruction>
        {
            Instruction.Create(OpCodes.Ldstr, method.DeclaringType.FullName),
            Instruction.Create(OpCodes.Ldstr, method.Name),
            Instruction.Create(OpCodes.Ldc_I4, method.Parameters.Count),
            Instruction.Create(OpCodes.Newarr, ModuleDefinition.TypeSystem.Object)
        };
        
        for (var i = 0; i < method.Parameters.Count; i++)
        {
            instructions.Add(Instruction.Create(OpCodes.Dup));
            instructions.Add(Instruction.Create(OpCodes.Ldc_I4, i));
            instructions.Add(Instruction.Create(OpCodes.Ldarg, method.Parameters[i]));

            if (method.Parameters[i].ParameterType.IsValueType)
            {
                instructions.Add(Instruction.Create(OpCodes.Box, method.Parameters[i].ParameterType));
            }

            instructions.Add(Instruction.Create(OpCodes.Stelem_Ref));
        }

        var cacheManagerType = ModuleDefinition.Types.First(t => t.Name == nameof(CacheManager));
        var getCacheValueMethod = cacheManagerType.Methods.First(m => m.Name == nameof(CacheManager.GetCacheValue));
        var getCacheValueMethodRef = ModuleDefinition.ImportReference(getCacheValueMethod);
        instructions.Add(Instruction.Create(OpCodes.Call, getCacheValueMethodRef));

        instructions.Add(Instruction.Create(OpCodes.Dup));
        var continueExecutionInstruction = Instruction.Create(OpCodes.Nop);
        instructions.Add(Instruction.Create(OpCodes.Brfalse_S, continueExecutionInstruction));

        if (method.ReturnType.IsValueType)
        {
            instructions.Add(Instruction.Create(OpCodes.Unbox_Any, method.ReturnType));
        }
        else
        {
            instructions.Add(Instruction.Create(OpCodes.Castclass, method.ReturnType));
        }
        instructions.Add(Instruction.Create(OpCodes.Ret));

        // Mark the point to continue execution if the result is null
        instructions.Add(continueExecutionInstruction);
        instructions.Add(Instruction.Create(OpCodes.Pop));


        return instructions;
    }

    private IReadOnlyCollection<Instruction> SetCacheValue(MethodDefinition method)
    {
        List<Instruction> instructions =
        [

            // Duplicate the return value
            Instruction.Create(OpCodes.Dup),
            Instruction.Create(OpCodes.Ldstr, method.DeclaringType.FullName),
            Instruction.Create(OpCodes.Ldstr, method.Name),
            Instruction.Create(OpCodes.Ldc_I4, method.Parameters.Count),
            Instruction.Create(OpCodes.Newarr, ModuleDefinition.TypeSystem.Object),
        ];

        for (var i = 0; i<method.Parameters.Count; i++)
        {
            instructions.Add(Instruction.Create(OpCodes.Dup));
            instructions.Add(Instruction.Create(OpCodes.Ldc_I4, i));
            instructions.Add(Instruction.Create(OpCodes.Ldarg, method.Parameters[i]));

            if (method.Parameters[i].ParameterType.IsValueType)
            {
                instructions.Add(Instruction.Create(OpCodes.Box, method.Parameters[i].ParameterType));
            }

            instructions.Add(Instruction.Create(OpCodes.Stelem_Ref));
        }
        
        
        var cacheManagerType = ModuleDefinition.Types.First(t => t.Name == nameof(CacheManager));
        var setCacheValue = cacheManagerType.Methods.First(m => m.Name == nameof(CacheManager.SetCacheValue));
        var setCacheValueReference = ModuleDefinition.ImportReference(setCacheValue);

        instructions.Add(Instruction.Create(OpCodes.Call, setCacheValueReference));
       
        return instructions;
    }

    private IReadOnlyCollection<Instruction> CaptureMethodResult(MethodDefinition method)
    {
        var instructions = new List<Instruction>();
        
        instructions.Add(Instruction.Create(OpCodes.Dup));

        var cacheManagerType = ModuleDefinition.Types.First(t => t.Name == nameof(CacheManager));
        var captureMethodResul = cacheManagerType.Methods.First(m => m.Name == nameof(CacheManager.CaptureMethodResult));
        var captureMethodResultReference = ModuleDefinition.ImportReference(captureMethodResul);

        instructions.Add(Instruction.Create(OpCodes.Call, captureMethodResultReference));

        return instructions;
    }

    #region ShouldCleanReference
    public override bool ShouldCleanReference => true;
    #endregion
}

#endregion