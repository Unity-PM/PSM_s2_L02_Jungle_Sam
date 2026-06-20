using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public static class MutantStalkerAnimatorControllerBuilder
{
    private const string ControllerPath = "Assets/JungleSam/Enemies/MutantStalker/Animators/AC_MutantStalker_Gameplay.controller";
    private const string MenuPath = "Tools/Jungle Sam/Enemies/Build Mutant Stalker Animator Controller";

    private const string Speed = "Speed";
    private const string IsDead = "IsDead";
    private const string IsActive = "IsActive";
    private const string IsRaging = "IsRaging";
    private const string Attack = "Attack";
    private const string GetHit = "GetHit";
    private const string Die = "Die";
    private const string Rage = "Rage";
    private const string IdleChange = "IdleChange";
    private const string AttackIndex = "AttackIndex";
    private const string HitIndex = "HitIndex";
    private const string DeathIndex = "DeathIndex";
    private const string IdleIndex = "IdleIndex";

    private static readonly string[] IdleClips =
    {
        "idle1",
        "idle2",
        "idle3",
        "idle4"
    };

    private static readonly string[] MovementClips =
    {
        "walk2",
        "walk3",
        "walk4",
        "walkback",
        "run1",
        "run2",
        "run3",
        "strafeleft",
        "straferight"
    };

    private static readonly string[] AttackClips =
    {
        "attack1",
        "attack1LSpike",
        "attack1RSpike",
        "attack2",
        "attack2LSpike",
        "attack2RLSpike",
        "attack3",
        "attack3RSpike",
        "attack4",
        "attack4RSpike",
        "attack5",
        "attack5LSpike"
    };

    private static readonly string[] HitClips =
    {
        "gethit1",
        "gethit2",
        "gethit3",
        "gethit4"
    };

    private static readonly string[] DeathClips =
    {
        "death1",
        "death2",
        "death3",
        "death4"
    };

    private static readonly string[] SpecialClips =
    {
        "rage",
        "jump"
    };

    [MenuItem(MenuPath)]
    private static void BuildController()
    {
        EnsureFolder("Assets/JungleSam");
        EnsureFolder("Assets/JungleSam/Enemies");
        EnsureFolder("Assets/JungleSam/Enemies/MutantStalker");
        EnsureFolder("Assets/JungleSam/Enemies/MutantStalker/Animators");

        if (AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(ControllerPath) != null)
            AssetDatabase.DeleteAsset(ControllerPath);

        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
        AddParameters(controller);

        AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;
        var states = new Dictionary<string, AnimatorState>();

        AddClipStates(stateMachine, states, IdleClips);
        AddClipStates(stateMachine, states, MovementClips);
        AddClipStates(stateMachine, states, AttackClips);
        AddClipStates(stateMachine, states, HitClips);
        AddClipStates(stateMachine, states, DeathClips);
        AddClipStates(stateMachine, states, SpecialClips);

        if (states.TryGetValue("idle1", out AnimatorState idleState))
            stateMachine.defaultState = idleState;
        else
            Debug.LogWarning("MutantStalker builder: idle1 clip was not found, default state was left unchanged.");

        AddLocomotionTransitions(states);
        AddIdleTransitions(stateMachine, states);
        AddAttackTransitions(stateMachine, states);
        AddHitTransitions(stateMachine, states);
        AddDeathTransitions(stateMachine, states);
        AddRageTransition(stateMachine, states);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"MutantStalker animator controller built: {ControllerPath}");
    }

    private static void AddParameters(AnimatorController controller)
    {
        controller.AddParameter(Speed, AnimatorControllerParameterType.Float);
        controller.AddParameter(IsDead, AnimatorControllerParameterType.Bool);
        controller.AddParameter(IsActive, AnimatorControllerParameterType.Bool);
        controller.AddParameter(IsRaging, AnimatorControllerParameterType.Bool);
        controller.AddParameter(Attack, AnimatorControllerParameterType.Trigger);
        controller.AddParameter(GetHit, AnimatorControllerParameterType.Trigger);
        controller.AddParameter(Die, AnimatorControllerParameterType.Trigger);
        controller.AddParameter(Rage, AnimatorControllerParameterType.Trigger);
        controller.AddParameter(IdleChange, AnimatorControllerParameterType.Trigger);
        controller.AddParameter(AttackIndex, AnimatorControllerParameterType.Int);
        controller.AddParameter(HitIndex, AnimatorControllerParameterType.Int);
        controller.AddParameter(DeathIndex, AnimatorControllerParameterType.Int);
        controller.AddParameter(IdleIndex, AnimatorControllerParameterType.Int);
    }

    private static void AddClipStates(
        AnimatorStateMachine stateMachine,
        Dictionary<string, AnimatorState> states,
        IReadOnlyList<string> clipNames)
    {
        foreach (string clipName in clipNames)
        {
            AnimationClip clip = FindClipByName(clipName);

            if (clip == null)
            {
                Debug.LogWarning($"MutantStalker builder: AnimationClip was not found: {clipName}");
                continue;
            }

            AnimatorState state = stateMachine.AddState(clipName);
            state.motion = clip;
            states[clipName] = state;
        }
    }

    private static void AddLocomotionTransitions(IReadOnlyDictionary<string, AnimatorState> states)
    {
        if (states.TryGetValue("idle1", out AnimatorState idle1) &&
            states.TryGetValue("walk2", out AnimatorState walk2))
        {
            AnimatorStateTransition idleToWalk = idle1.AddTransition(walk2);
            ConfigureLocomotionTransition(idleToWalk);
            idleToWalk.AddCondition(AnimatorConditionMode.Greater, 0.1f, Speed);
            idleToWalk.AddCondition(AnimatorConditionMode.Less, 3.5f, Speed);
            idleToWalk.AddCondition(AnimatorConditionMode.IfNot, 0f, IsDead);

            AnimatorStateTransition walkToIdle = walk2.AddTransition(idle1);
            ConfigureLocomotionTransition(walkToIdle);
            walkToIdle.AddCondition(AnimatorConditionMode.Less, 0.1f, Speed);
            walkToIdle.AddCondition(AnimatorConditionMode.IfNot, 0f, IsDead);
        }

        if (states.TryGetValue("walk2", out AnimatorState walkState) &&
            states.TryGetValue("run1", out AnimatorState run1))
        {
            AnimatorStateTransition walkToRun = walkState.AddTransition(run1);
            ConfigureLocomotionTransition(walkToRun);
            walkToRun.AddCondition(AnimatorConditionMode.Greater, 3.5f, Speed);
            walkToRun.AddCondition(AnimatorConditionMode.IfNot, 0f, IsDead);

            AnimatorStateTransition runToWalk = run1.AddTransition(walkState);
            ConfigureLocomotionTransition(runToWalk);
            runToWalk.AddCondition(AnimatorConditionMode.Less, 3.5f, Speed);
            runToWalk.AddCondition(AnimatorConditionMode.Greater, 0.1f, Speed);
            runToWalk.AddCondition(AnimatorConditionMode.IfNot, 0f, IsDead);
        }

        if (states.TryGetValue("idle1", out AnimatorState idleState) &&
            states.TryGetValue("run1", out AnimatorState runState))
        {
            AnimatorStateTransition idleToRun = idleState.AddTransition(runState);
            ConfigureLocomotionTransition(idleToRun);
            idleToRun.AddCondition(AnimatorConditionMode.Greater, 3.5f, Speed);
            idleToRun.AddCondition(AnimatorConditionMode.IfNot, 0f, IsDead);

            AnimatorStateTransition runToIdle = runState.AddTransition(idleState);
            ConfigureLocomotionTransition(runToIdle);
            runToIdle.AddCondition(AnimatorConditionMode.Less, 0.1f, Speed);
            runToIdle.AddCondition(AnimatorConditionMode.IfNot, 0f, IsDead);
        }
    }

    private static void AddIdleTransitions(AnimatorStateMachine stateMachine, IReadOnlyDictionary<string, AnimatorState> states)
    {
        for (int i = 0; i < IdleClips.Length; i++)
        {
            string clipName = IdleClips[i];

            if (!states.TryGetValue(clipName, out AnimatorState state))
                continue;

            AnimatorStateTransition transition = stateMachine.AddAnyStateTransition(state);
            ConfigureInstantTransition(transition);
            transition.canTransitionToSelf = false;
            transition.AddCondition(AnimatorConditionMode.If, 0f, IdleChange);
            transition.AddCondition(AnimatorConditionMode.Equals, i + 1, IdleIndex);
            transition.AddCondition(AnimatorConditionMode.IfNot, 0f, IsActive);
            transition.AddCondition(AnimatorConditionMode.IfNot, 0f, IsDead);
        }
    }

    private static void AddAttackTransitions(AnimatorStateMachine stateMachine, IReadOnlyDictionary<string, AnimatorState> states)
    {
        for (int i = 0; i < AttackClips.Length; i++)
        {
            string clipName = AttackClips[i];

            if (!states.TryGetValue(clipName, out AnimatorState state))
                continue;

            AnimatorStateTransition anyStateTransition = stateMachine.AddAnyStateTransition(state);
            ConfigureActionEnterTransition(anyStateTransition);
            anyStateTransition.AddCondition(AnimatorConditionMode.If, 0f, Attack);
            anyStateTransition.AddCondition(AnimatorConditionMode.Equals, i + 1, AttackIndex);
            anyStateTransition.AddCondition(AnimatorConditionMode.IfNot, 0f, IsDead);

            AddExitTransitionToIdle(states, state, 0.98f, 0.03f);
        }
    }

    private static void AddHitTransitions(AnimatorStateMachine stateMachine, IReadOnlyDictionary<string, AnimatorState> states)
    {
        for (int i = 0; i < HitClips.Length; i++)
        {
            string clipName = HitClips[i];

            if (!states.TryGetValue(clipName, out AnimatorState state))
                continue;

            AnimatorStateTransition anyStateTransition = stateMachine.AddAnyStateTransition(state);
            ConfigureActionEnterTransition(anyStateTransition);
            anyStateTransition.AddCondition(AnimatorConditionMode.If, 0f, GetHit);
            anyStateTransition.AddCondition(AnimatorConditionMode.Equals, i + 1, HitIndex);
            anyStateTransition.AddCondition(AnimatorConditionMode.IfNot, 0f, IsDead);

            AddExitTransitionToIdle(states, state, 0.95f, 0.03f);
        }
    }

    private static void AddDeathTransitions(AnimatorStateMachine stateMachine, IReadOnlyDictionary<string, AnimatorState> states)
    {
        for (int i = 0; i < DeathClips.Length; i++)
        {
            string clipName = DeathClips[i];

            if (!states.TryGetValue(clipName, out AnimatorState state))
                continue;

            AnimatorStateTransition transition = stateMachine.AddAnyStateTransition(state);
            ConfigureActionEnterTransition(transition);
            transition.AddCondition(AnimatorConditionMode.If, 0f, Die);
            transition.AddCondition(AnimatorConditionMode.Equals, i + 1, DeathIndex);
        }
    }

    private static void AddRageTransition(AnimatorStateMachine stateMachine, IReadOnlyDictionary<string, AnimatorState> states)
    {
        if (!states.TryGetValue("rage", out AnimatorState rageState))
            return;

        AnimatorStateTransition anyStateTransition = stateMachine.AddAnyStateTransition(rageState);
        ConfigureActionEnterTransition(anyStateTransition);
        anyStateTransition.AddCondition(AnimatorConditionMode.If, 0f, Rage);
        anyStateTransition.AddCondition(AnimatorConditionMode.IfNot, 0f, IsDead);

        if (states.TryGetValue("run1", out AnimatorState runState))
        {
            AnimatorStateTransition rageToRun = rageState.AddTransition(runState);
            rageToRun.hasExitTime = true;
            rageToRun.exitTime = 0.9f;
            rageToRun.duration = 0.05f;
        }
    }

    private static void AddExitTransitionToIdle(
        IReadOnlyDictionary<string, AnimatorState> states,
        AnimatorState fromState,
        float exitTime,
        float duration)
    {
        if (!states.TryGetValue("idle1", out AnimatorState idleState))
            return;

        AnimatorStateTransition transition = fromState.AddTransition(idleState);
        transition.hasExitTime = true;
        transition.exitTime = exitTime;
        transition.duration = duration;
        transition.interruptionSource = TransitionInterruptionSource.None;
    }

    private static void ConfigureInstantTransition(AnimatorStateTransition transition)
    {
        transition.hasExitTime = false;
        transition.exitTime = 0f;
        transition.duration = 0.05f;
        transition.interruptionSource = TransitionInterruptionSource.None;
    }

    private static void ConfigureLocomotionTransition(AnimatorStateTransition transition)
    {
        transition.hasExitTime = false;
        transition.exitTime = 0f;
        transition.duration = 0.03f;
        transition.interruptionSource = TransitionInterruptionSource.None;
    }

    private static void ConfigureActionEnterTransition(AnimatorStateTransition transition)
    {
        transition.hasExitTime = false;
        transition.exitTime = 0f;
        transition.duration = 0f;
        transition.canTransitionToSelf = false;
        transition.interruptionSource = TransitionInterruptionSource.None;
    }

    private static AnimationClip FindClipByName(string clipName)
    {
        string[] guids = AssetDatabase.FindAssets($"{clipName} t:AnimationClip", new[] { "Assets" });

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);

            if (clip != null && clip.name == clipName)
                return clip;

            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
            foreach (Object asset in assets)
            {
                if (asset is AnimationClip nestedClip && nestedClip.name == clipName)
                    return nestedClip;
            }
        }

        return null;
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
            return;

        string parent = Path.GetDirectoryName(path);
        string folderName = Path.GetFileName(path);

        if (string.IsNullOrEmpty(parent) || string.IsNullOrEmpty(folderName))
            return;

        EnsureFolder(parent.Replace("\\", "/"));
        AssetDatabase.CreateFolder(parent.Replace("\\", "/"), folderName);
    }
}
