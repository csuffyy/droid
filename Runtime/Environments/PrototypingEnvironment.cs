﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Neodroid.Runtime.Interfaces;
using Neodroid.Runtime.Messaging.Messages;
using Neodroid.Runtime.Prototyping.Evaluation;
using Neodroid.Runtime.Utilities.BoundingBoxes;
using Neodroid.Runtime.Utilities.Enums;
using Neodroid.Runtime.Utilities.EventRecipients.droid.Neodroid.Utilities.Unsorted;
using Neodroid.Runtime.Utilities.Misc;
using Neodroid.Runtime.Utilities.Misc.Extensions;
using UnityEngine;
using Object = System.Object;

namespace Neodroid.Runtime.Environments {
  /// <inheritdoc cref="NeodroidEnvironment" />
  /// <summary>
  ///   Environment to be used with the prototyping components.
  /// </summary>
  [AddComponentMenu("Neodroid/Environments/PrototypingEnvironment")]
  public class PrototypingEnvironment : NeodroidEnvironment,
                                        IPrototypingEnvironment {
    #region NeodroidCallbacks

    /// <inheritdoc />
    /// <summary>
    /// </summary>
    protected override void PreSetup() {
      if (this._objective_function == null) {
        this._objective_function = this.GetComponent<ObjectiveFunction>();
      }

      if (!this.PlayableArea) {
        this.PlayableArea = this.GetComponent<BoundingBox>();
      }

      #if NEODROID_DEBUG
      if (this.Debugging) {
        Debug.Log("Setting up");
      }
      #endif

      if (this._tracked_game_objects == null || this._tracked_game_objects.Length == 0) {
        this.SaveInitialPoses();
        this.SaveInitialAnimations();
        this.StartCoroutine(this.SaveInitialBodiesIe());
      }
    }

    /// <inheritdoc />
    /// <summary>
    /// </summary>
    protected override void Clear() {
      this.Displayers.Clear();
      this.Configurables.Clear();
      this.Actors.Clear();
      this.Observers.Clear();
      this.Resetables.Clear();
      this.Listeners.Clear();
    }

    /// <inheritdoc />
    /// <summary>
    /// </summary>
    public override void PostStep() {
      this.PostStepEvent?.Invoke();
      if (this._Configure) {
        #if NEODROID_DEBUG
        if (this.Debugging) {
          Debug.Log("Configuring");
        }
        #endif
        this._Configure = false;
        this.Configure();
      }

      if (!this._Simulation_Manager.IsSyncingEnvironments) {
        this._ReplyWithDescriptionThisStep = false;
      }
    }

    /// <inheritdoc />
    /// <summary>
    /// </summary>
    /// <returns></returns>
    public override Reaction SampleReaction() {
      #if NEODROID_DEBUG
      if (this.Debugging) {
        Debug.Log($"Sampling a reaction for environment {this.Identifier}");
      }
      #endif

      this._sample_motions.Clear();

      foreach (var actor in this.Actors) {
        var actor_value = actor.Value;
        if (actor_value != null && actor_value.Motors != null) {
          foreach (var motor in actor_value.Motors) {
            var motor_value = motor.Value;
            if (motor_value != null) {
              this._sample_motions.Add(new MotorMotion(actor.Key, motor.Key, motor_value.Sample()));
            }
          }
        }
      }

      if (this._Terminated) {
        #if NEODROID_DEBUG
        if (this.Debugging) {
          Debug.Log("SampleReaction resetting environment");
        }
        #endif

        var reset_reaction =
            new ReactionParameters(false, false, true, episode_count : true) {IsExternal = false};
        return new Reaction(reset_reaction, this.Identifier);
      }

      var rp = new ReactionParameters(true, true, episode_count : true) {IsExternal = false};
      return new Reaction(rp, this._sample_motions.ToArray(), null, null, null, "", this.Identifier);
    }

    /// <summary>
    /// </summary>
    protected void UpdateObserversData() {
      foreach (var obs in this.Observers.Values) {
        obs?.UpdateObservation();
      }
    }

    /// <summary>
    /// </summary>
    protected void UpdateConfigurableValues() {
      foreach (var con in this.Configurables.Values) {
        con?.UpdateCurrentConfiguration();
      }
    }

    #endregion

    #region Fields

    /// <summary>
    /// </summary>
    [Header("References", order = 20)]
    [SerializeField]
    ObjectiveFunction _objective_function;

    /// <summary>
    /// </summary>
    [Header("General", order = 30)]
    [SerializeField]
    Transform _coordinate_reference_point;

    /// <summary>
    /// </summary>
    [SerializeField]
    bool _track_only_children = true;

    /// <summary>
    /// </summary>
    [SerializeField]
    CoordinateSystem _coordinate_system = CoordinateSystem.Local_coordinates_;

    /// <summary>
    /// </summary>
    [Header("(Optional)", order = 80)]
    [SerializeField]
    BoundingBox _playable_area;

    /// <summary>
    /// </summary>
    Object _reaction_lock = new Object();

    [SerializeField] int _reset_i;

    WaitForFixedUpdate _wait_for_fixed_update = new WaitForFixedUpdate();
    List<float> _observables = new List<float>();
    List<MotorMotion> _sample_motions = new List<MotorMotion>();

    #endregion

    #region PrivateMembers

    /// <summary>
    /// </summary>
    Vector3[] _reset_positions;

    /// <summary>
    /// </summary>
    Quaternion[] _reset_rotations;

    /// <summary>
    /// </summary>
    GameObject[] _tracked_game_objects;

    /// <summary>
    /// </summary>
    Vector3[] _reset_velocities;

    /// <summary>
    /// </summary>
    Vector3[] _reset_angulars;

    /// <summary>
    /// </summary>
    Rigidbody[] _tracked_rigid_bodies;

    /// <summary>
    /// </summary>
    Transform[] _poses;

    /// <summary>
    /// </summary>
    Pose[] _received_poses;

    /// <summary>
    /// </summary>
    Body[] _received_bodies;

    /// <summary>
    /// </summary>
    Configuration[] _received_configurations;

    /// <summary>
    /// </summary>
    Animation[] _animations;

    /// <summary>
    /// </summary>
    float[] _reset_animation_times;

    [SerializeField] bool _initials_saved_flag;
    [SerializeField] bool _update_observations_with_every_tick;

    #endregion

    #region PublicMethods

    #region Getters

    /// <summary>
    /// </summary>
    public Dictionary<string, IDisplayer> Displayers { get; } = new Dictionary<string, IDisplayer>();

    /// <summary>
    /// </summary>
    public Dictionary<string, IConfigurable> Configurables { get; } = new Dictionary<string, IConfigurable>();

    /// <summary>
    /// </summary>
    public Dictionary<string, IActor> Actors { get; } = new Dictionary<string, IActor>();

    /// <summary>
    /// </summary>
    public SortedDictionary<string, IObserver> Observers { get; } = new SortedDictionary<string, IObserver>();

    /// <summary>
    /// </summary>
    public Dictionary<string, IResetable> Resetables { get; } = new Dictionary<string, IResetable>();

    /// <summary>
    /// </summary>
    public Dictionary<string, IEnvironmentListener> Listeners { get; } =
      new Dictionary<string, IEnvironmentListener>();

    /// <inheritdoc />
    /// <summary>
    /// </summary>
    public override string PrototypingTypeName { get { return "PrototypingEnvironment"; } }

    /// <summary>
    /// </summary>
    public IObjective ObjectiveFunction {
      get { return this._objective_function; }
      set { this._objective_function = (ObjectiveFunction)value; }
    }

    /// <summary>
    /// </summary>
    public Transform Transform { get { return this.transform; } }

    /// <summary>
    /// </summary>
    public BoundingBox PlayableArea {
      get { return this._playable_area; }
      set { this._playable_area = value; }
    }

    /// <summary>
    /// </summary>
    public Transform CoordinateReferencePoint {
      get { return this._coordinate_reference_point; }
      set { this._coordinate_reference_point = value; }
    }

    /// <summary>
    /// </summary>
    public CoordinateSystem CoordinateSystem {
      get { return this._coordinate_system; }
      set { this._coordinate_system = value; }
    }

    #endregion

    /// <summary>
    ///   Termination of an episode, can be supplied with a reason for various purposes debugging or clarification
    ///   for a learner.
    /// </summary>
    /// <param name="reason"></param>
    public void Terminate(string reason = "None") {
      lock (this._reaction_lock) {
        if (this._Terminable) {
          #if NEODROID_DEBUG
          if (this.Debugging) {
            Debug.LogWarning($"Environment {this.Identifier} as terminated because {reason}");
          }
          #endif

          this._Terminated = true;
          this._LastTermination_Reason = reason;
        }
      }
    }

    /// <inheritdoc />
    /// <summary>
    /// </summary>
    /// <param name="reaction"></param>
    /// <returns></returns>
    public override EnvironmentState ReactAndCollectState(Reaction reaction) {
      this.React(reaction);

      return this.CollectState();
    }

    /// <inheritdoc />
    /// <summary>
    /// </summary>
    /// <param name="reaction"></param>
    /// <returns></returns>
    public override void React(Reaction reaction) {
      lock (this._reaction_lock) {
        this.LastReaction = reaction;
        this._Terminable = reaction.Parameters.Terminable;

        if (reaction.Parameters.IsExternal) {
          this._received_configurations = reaction.Configurations;

          if (!this._ReplyWithDescriptionThisStep) {
            this._ReplyWithDescriptionThisStep = reaction.Parameters.Describe;
          }

          this._Configure = reaction.Parameters.Configure;
          if (this._Configure && reaction.Unobservables != null) {
            this._received_poses = reaction.Unobservables.Poses;
            this._received_bodies = reaction.Unobservables.Bodies;
          }
        }

        this.SendToDisplayers(reaction);

        if (reaction.Parameters.Reset) {
          this.Terminate(
              $"{(reaction.Parameters.IsExternal ? "External" : "Internal")} reaction caused a reset");
          this._Resetting = true;
        } else if (reaction.Parameters.Step) {
          #if NEODROID_DEBUG
          if (this.Debugging) {
            Debug.Log($"Stepping in environment({this.Identifier})");
          }
          #endif
          this.Step(reaction);
        }
      }
    }

    /// <inheritdoc />
    /// <summary>
    /// </summary>
    public override void Tick() {
      if (this._Resetting) {
        if (this._reset_i >= this._Simulation_Manager.SimulatorConfiguration.ResetIterations) {
          this._Resetting = false;
          this._reset_i = 0;
          this.UpdateConfigurableValues();
          this.UpdateObserversData();
        } else {
          this.EnvironmentReset();
          this._reset_i += 1;
        }

        #if NEODROID_DEBUG
        if (this.Debugging) {
          Debug.Log(
              $"Reset {this._reset_i}/{this._Simulation_Manager.SimulatorConfiguration.ResetIterations}");
        }
        #endif
      } else {
        if (this._update_observations_with_every_tick) {
          this.UpdateObserversData();
        }
      }

      #if NEODROID_DEBUG
      if (this.Debugging) {
        Debug.Log("Tick");
      }
      #endif
    }

    #region Registration

    /// <inheritdoc />
    /// <summary>
    /// </summary>
    public void Register(IDisplayer displayer) { this.Register(displayer, displayer.Identifier); }

    /// <inheritdoc />
    /// <summary>
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="identifier"></param>
    public void Register(IDisplayer obj, string identifier) {
      if (!this.Displayers.ContainsKey(identifier)) {
        #if NEODROID_DEBUG
        if (this.Debugging) {
          Debug.Log($"Environment {this.name} has registered displayer {identifier}");
        }
        #endif
        this.Displayers.Add(identifier, obj);
      } else {
        Debug.LogWarning(
            $"WARNING! Please check for duplicates, Environment {this.name} already has displayer {identifier} registered");
      }
    }

    /// <inheritdoc />
    /// <summary>
    /// </summary>
    /// <param name="actor"></param>
    public void Register(IActor actor) { this.Register(actor, actor.Identifier); }

    /// <inheritdoc />
    /// <summary>
    /// </summary>
    /// <param name="actor"></param>
    /// <param name="identifier"></param>
    public void Register(IActor actor, string identifier) {
      if (!this.Actors.ContainsKey(identifier)) {
        #if NEODROID_DEBUG
        if (this.Debugging) {
          Debug.Log($"Environment {this.name} has registered actor {identifier}");
        }
        #endif

        this.Actors.Add(identifier, actor);
      } else {
        Debug.LogWarning(
            $"WARNING! Please check for duplicates, Environment {this.name} already has actor {identifier} registered");
      }
    }

    /// <inheritdoc />
    /// <summary>
    /// </summary>
    /// <param name="observer"></param>
    public void Register(IObserver observer) { this.Register(observer, observer.Identifier); }

    /// <inheritdoc />
    /// ///
    /// <summary>
    /// </summary>
    /// <param name="observer"></param>
    /// <param name="identifier"></param>
    public void Register(IObserver observer, string identifier) {
      if (!this.Observers.ContainsKey(identifier)) {
        #if NEODROID_DEBUG
        if (this.Debugging) {
          Debug.Log($"Environment {this.name} has registered observer {identifier}");
        }
        #endif

        this.Observers.Add(identifier, observer);
      } else {
        Debug.LogWarning(
            $"WARNING! Please check for duplicates, Environment {this.name} already has observer {identifier} registered");
      }
    }

    /// <inheritdoc />
    /// <summary>
    /// </summary>
    /// <param name="configurable"></param>
    public void Register(IConfigurable configurable) { this.Register(configurable, configurable.Identifier); }

    /// <inheritdoc />
    /// <summary>
    /// </summary>
    /// <param name="configurable"></param>
    /// <param name="identifier"></param>
    public void Register(IConfigurable configurable, string identifier) {
      if (!this.Configurables.ContainsKey(identifier)) {
        #if NEODROID_DEBUG
        if (this.Debugging) {
          Debug.Log($"Environment {this.name} has registered configurable {identifier}");
        }
        #endif

        this.Configurables.Add(identifier, configurable);
      } else {
        Debug.LogWarning(
            $"WARNING! Please check for duplicates, Environment {this.name} already has configurable {identifier} registered");
      }
    }

    /// <inheritdoc />
    /// <summary>
    /// </summary>
    /// <param name="resetable"></param>
    /// <param name="identifier"></param>
    public void Register(IResetable resetable, string identifier) {
      if (!this.Resetables.ContainsKey(identifier)) {
        #if NEODROID_DEBUG
        if (this.Debugging) {
          Debug.Log($"Environment {this.name} has registered resetable {identifier}");
        }
        #endif
        this.Resetables.Add(identifier, resetable);
      } else {
        #if NEODROID_DEBUG
        if (this.Debugging) {
          Debug.Log(
              $"WARNING! Please check for duplicates, Environment {this.name} already has resetable {identifier} registered");
        }
        #endif
      }
    }

    /// <inheritdoc />
    /// <summary>
    /// </summary>
    /// <param name="resetable"></param>
    public void Register(IResetable resetable) { this.Register(resetable, resetable.Identifier); }

    public void Register(IEnvironmentListener environment_listener) {
      this.Register(environment_listener, environment_listener.Identifier);
    }

    public void Register(IEnvironmentListener environment_listener, string identifier) {
      if (!this.Listeners.ContainsKey(identifier)) {
        #if NEODROID_DEBUG
        if (this.Debugging) {
          Debug.Log($"Environment {this.name} has registered listener {identifier}");
        }
        #endif
        this.Listeners.Add(identifier, environment_listener);
      } else {
        #if NEODROID_DEBUG
        if (this.Debugging) {
          Debug.Log(
              $"WARNING! Please check for duplicates, Environment {this.name} already has listener {identifier} registered");
        }
        #endif
      }
    }

    /// <summary>
    /// </summary>
    /// <param name="actor"></param>
    public void UnRegister(IActor actor) { this.UnRegister(actor, actor.Identifier); }

    public void UnRegister(IActor t, string obj) {
      if (this.Actors.ContainsKey(obj)) {
        #if NEODROID_DEBUG
        if (this.Debugging) {
          Debug.Log($"Environment {this.name} unregistered actor {obj}");
        }
        #endif
        this.Actors.Remove(obj);
      }
    }

    /// <summary>
    /// </summary>
    /// <param name="observer"></param>
    public void UnRegister(IObserver observer) { this.UnRegister(observer, observer.Identifier); }

    public void UnRegister(IObserver t, string identifier) {
      if (this.Observers.ContainsKey(identifier)) {
        #if NEODROID_DEBUG
        if (this.Debugging) {
          Debug.Log($"Environment {this.name} unregistered observer {identifier}");
        }
        #endif
        this.Observers.Remove(identifier);
      }
    }

    /// <summary>
    /// </summary>
    /// <param name="configurable"></param>
    public void UnRegister(IConfigurable configurable) {
      this.UnRegister(configurable, configurable.Identifier);
    }

    public void UnRegister(IConfigurable t, string identifier) {
      if (this.Configurables.ContainsKey(identifier)) {
        #if NEODROID_DEBUG
        if (this.Debugging) {
          Debug.Log($"Environment {this.name} unregistered configurable {identifier}");
        }
        #endif
        this.Configurables.Remove(identifier);
      }
    }

    /// <summary>
    /// </summary>
    /// <param name="displayer"></param>
    public void UnRegister(IDisplayer displayer) { this.UnRegister(displayer, displayer.Identifier); }

    public void UnRegister(IDisplayer t, string identifier) {
      if (this.Displayers.ContainsKey(identifier)) {
        #if NEODROID_DEBUG
        if (this.Debugging) {
          Debug.Log($"Environment {this.name} unregistered configurable {identifier}");
        }
        #endif
        this.Displayers.Remove(identifier);
      }
    }

    /// <summary>
    /// </summary>
    /// <param name="resetable"></param>
    public void UnRegister(IResetable resetable) { this.UnRegister(resetable, resetable.Identifier); }

    public void UnRegister(IResetable t, string identifier) {
      if (this.Resetables.ContainsKey(identifier)) {
        #if NEODROID_DEBUG
        if (this.Debugging) {
          Debug.Log($"Environment {this.name} unregistered resetable {identifier}");
        }
        #endif
        this.Resetables.Remove(identifier);
      }
    }

    /// <summary>
    /// </summary>
    /// <param name="environment_listener"></param>
    public void UnRegister(IEnvironmentListener environment_listener) {
      this.UnRegister(environment_listener, environment_listener.Identifier);
    }

    public void UnRegister(IEnvironmentListener t, string identifier) {
      if (this.Listeners.ContainsKey(identifier)) {
        #if NEODROID_DEBUG
        if (this.Debugging) {
          Debug.Log($"Environment {this.name} unregistered listener {identifier}");
        }
        #endif
        this.Listeners.Remove(identifier);
      }
    }

    #endregion

    #region Transformations

    /// <summary>
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public Vector3 TransformPosition(Vector3 position) {
      if (this._coordinate_system == CoordinateSystem.Relative_to_reference_point_) {
        if (this._coordinate_reference_point) {
          return this._coordinate_reference_point.transform.InverseTransformPoint(position);
        }

        return position;
      }

      if (this._coordinate_system == CoordinateSystem.Local_coordinates_) {
        return position - this.transform.position;
      }

      return position;
    }

    /// <summary>
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public Vector3 InverseTransformPosition(Vector3 position) {
      if (this._coordinate_system == CoordinateSystem.Relative_to_reference_point_) {
        if (this._coordinate_reference_point) {
          return this._coordinate_reference_point.transform.TransformPoint(position);
        }

        return position;
      }

      if (this._coordinate_system == CoordinateSystem.Local_coordinates_) {
        return position - this.transform.position;
      }

      return position;
    }

    /// <summary>
    /// </summary>
    /// <param name="direction"></param>
    /// <returns></returns>
    public Vector3 TransformDirection(Vector3 direction) {
      if (this._coordinate_system == CoordinateSystem.Relative_to_reference_point_) {
        if (this._coordinate_reference_point) {
          return this._coordinate_reference_point.transform.InverseTransformDirection(direction);
        }

        return direction;
      }

      if (this._coordinate_system == CoordinateSystem.Local_coordinates_) {
        return this.transform.InverseTransformDirection(direction);
      }

      return direction;
    }

    /// <summary>
    /// </summary>
    /// <param name="direction"></param>
    /// <returns></returns>
    public Vector3 InverseTransformDirection(Vector3 direction) {
      if (this._coordinate_system == CoordinateSystem.Relative_to_reference_point_) {
        if (this._coordinate_reference_point) {
          return this._coordinate_reference_point.transform.TransformDirection(direction);
        }

        return direction;
      }

      if (this._coordinate_system == CoordinateSystem.Local_coordinates_) {
        return this.transform.InverseTransformDirection(direction);
      }

      return direction;
    }

    /// <summary>
    /// </summary>
    /// <param name="quaternion"></param>
    /// <returns></returns>
    public Quaternion TransformRotation(Quaternion quaternion) {
      if (this._coordinate_system == CoordinateSystem.Relative_to_reference_point_) {
        if (this._coordinate_reference_point) {
          return Quaternion.Inverse(this._coordinate_reference_point.rotation) * quaternion;
        }

        //Quaternion.Euler(this._coordinate_reference_point.transform.TransformDirection(quaternion.forward));
      }

      return quaternion;
    }

    #endregion

    #endregion

    #region PrivateMethods

    /// <summary>
    /// </summary>
    void SaveInitialPoses() {
      var ignored_layer = LayerMask.NameToLayer("IgnoredByNeodroid");
      if (this._track_only_children) {
        this._tracked_game_objects =
            NeodroidUtilities.RecursiveChildGameObjectsExceptLayer(this.transform, ignored_layer);
      } else {
        this._tracked_game_objects = NeodroidUtilities.FindAllGameObjectsExceptLayer(ignored_layer);
      }

      var length = this._tracked_game_objects.Length;

      this._reset_positions = new Vector3[length];
      this._reset_rotations = new Quaternion[length];
      this._poses = new Transform[length];
      for (var i = 0; i < length; i++) {
        var go = this._tracked_game_objects[i];
        var trans = go.transform;
        this._reset_positions[i] = trans.position;
        this._reset_rotations[i] = trans.rotation;
        this._poses[i] = trans;
        var maybe_joint = go.GetComponent<Joint>();
        if (maybe_joint != null) {
          var maybe_joint_fix = maybe_joint.GetComponent<JointFix>();
          if (maybe_joint_fix == null) {
            // ReSharper disable once RedundantAssignment
            maybe_joint_fix = maybe_joint.gameObject.AddComponent<JointFix>();
          }
          #if NEODROID_DEBUG
          if (this.Debugging) {
            Debug.Log($"Added a JointFix component to {maybe_joint_fix.name}");
          }
          #endif
        }
      }
    }

    /// <summary>
    /// </summary>
    void SaveInitialBodies() {
      /*var body_list = new List<Rigidbody>();
      foreach (var go in this._tracked_game_objects) {
        if (go != null) {
          var body = go.GetComponent<Rigidbody>();
          if (body)
            body_list.Add(body);
        }
      }
            this._bodies = body_list.ToArray();
      */ //Should be equalvalent to the line below, but kept as a reference in case of confusion

      this._tracked_rigid_bodies = this._tracked_game_objects.Where(go => go != null)
          .Select(go => go.GetComponent<Rigidbody>()).Where(body => body).ToArray();

      this._reset_velocities = new Vector3[this._tracked_rigid_bodies.Length];
      this._reset_angulars = new Vector3[this._tracked_rigid_bodies.Length];
      for (var i = 0; i < this._tracked_rigid_bodies.Length; i++) {
        this._reset_velocities[i] = this._tracked_rigid_bodies[i].velocity;
        this._reset_angulars[i] = this._tracked_rigid_bodies[i].angularVelocity;
      }
    }

    /// <summary>
    /// </summary>
    void SaveInitialAnimations() {
      this._animations = this._tracked_game_objects.Where(go => go != null)
          .Select(go => go.GetComponent<Animation>()).Where(anim => anim).ToArray();
      this._reset_animation_times = new float[this._animations.Length];
      for (var i = 0; i < this._animations.Length; i++) {
        if (this._animations[i]) {
          if (this._animations[i].clip) {
            this._reset_animation_times[i] =
                this._animations[i].CrossFadeQueued(this._animations[i].clip.name)
                    .time; //TODO: IS NOT USED AS RIGHT NOW and should use animations clips instead the legacy "clip.name".
            //TODO: DOES NOT WORK
          }
        }
      }
    }

    /// <summary>
    /// </summary>
    /// <returns></returns>
    IEnumerator SaveInitialBodiesIe() {
      yield return this._wait_for_fixed_update;
      this.SaveInitialBodies();
    }

    /// <inheritdoc />
    /// <summary>
    /// </summary>
    /// <returns></returns>
    public override EnvironmentState CollectState() {
      lock (this._reaction_lock) {
        if (this.Actors != null) {
          foreach (var a in this.Actors.Values) {
            if (a.Motors != null) {
              foreach (var m in a.Motors.Values) {
                this._Energy_Spent += m.GetEnergySpend();
              }
            }
          }
        }

        var signal = 0f;

        if (this._objective_function != null) {
          signal = this._objective_function.Evaluate();
        }

        EnvironmentDescription description = null;
        if (this._ReplyWithDescriptionThisStep) {
          #if NEODROID_DEBUG
          if (this.Debugging) {
            Debug.Log("Describing Environment");
          }
          #endif
          var threshold = 0f;
          if (this._objective_function != null) {
            threshold = this._objective_function.SolvedThreshold;
          }

          description = new EnvironmentDescription(
              this.EpisodeLength,
              this.Actors,
              this.Configurables,
              threshold);
        }

        this._observables.Clear();
        foreach (var item in this.Observers) {
          if (item.Value != null) {
            if (item.Value.FloatEnumerable != null) {
              this._observables.AddRange(item.Value.FloatEnumerable);
            } else {
              #if NEODROID_DEBUG
              if (this.Debugging) {
                Debug.Log($"Observer with key {item.Key} has a null FloatEnumerable value");
              }
              #endif
            }
          } else {
            #if NEODROID_DEBUG
            if (this.Debugging) {
              Debug.Log($"Observer with key {item.Key} has a null value");
            }
            #endif
          }
        }

        var obs = this._observables.ToArray();

        var time = Time.time - this._Lastest_Reset_Time;

        var state = new EnvironmentState(
            this.Identifier,
            this._Energy_Spent,
            this.CurrentFrameNumber,
            time,
            signal,
            this._Terminated,
            ref obs,
            this.LastTerminationReason,
            description);

        if (this._Simulation_Manager.SimulatorConfiguration.AlwaysSerialiseUnobservables
            || this._ReplyWithDescriptionThisStep) {
          state.Unobservables = new Unobservables(ref this._tracked_rigid_bodies, ref this._poses);
        }

        if (this._Simulation_Manager.SimulatorConfiguration.AlwaysSerialiseIndividualObservables
            || this._ReplyWithDescriptionThisStep) {
          state.Observers = this.Observers.Values.ToArray();
        }

        return state;
      }
    }

    /// <inheritdoc />
    /// <summary>
    /// </summary>
    /// <param name="recipient"></param>
    public override void ObservationsString(DataPoller recipient) {
      recipient.PollData(
          string.Join("\n\n", this.Observers.Values.Select(e => $"{e.Identifier}:\n{e.ToString()}")));
    }

    /// <summary>
    /// </summary>
    public override void EnvironmentReset() {
      this._Lastest_Reset_Time = Time.time;
      this.CurrentFrameNumber = 0;
      this._objective_function?.EnvironmentReset();

      this.SetEnvironmentPoses(this._tracked_game_objects, this._reset_positions, this._reset_rotations);
      this.SetEnvironmentBodies(this._tracked_rigid_bodies, this._reset_velocities, this._reset_angulars);

      this.ResetRegisteredObjects();
      this.Configure();

      #if NEODROID_DEBUG
      if (this.Debugging) {
        Debug.Log($"Reset called on environment {this.Identifier}");
      }
      #endif

      this._Terminated = false;
    }

    /// <summary>
    /// </summary>
    protected void Configure() {
      #if NEODROID_DEBUG
      if (this.Debugging) {
        Debug.Log("Configure was called");
      }
      #endif

      if (this._received_poses != null) {
        var positions = new Vector3[this._received_poses.Length];
        var rotations = new Quaternion[this._received_poses.Length];
        for (var i = 0; i < this._received_poses.Length; i++) {
          positions[i] = this._received_poses[i].position;
          rotations[i] = this._received_poses[i].rotation;
        }

        this.SetEnvironmentPoses(this._tracked_game_objects, positions, rotations);
      }

      if (this._received_bodies != null) {
        var velocities = new Vector3[this._received_bodies.Length];
        var angulars = new Vector3[this._received_bodies.Length];
        for (var i = 0; i < this._received_bodies.Length; i++) {
          velocities[i] = this._received_bodies[i].Velocity;
          angulars[i] = this._received_bodies[i].AngularVelocity;
        }

        this.SetEnvironmentBodies(this._tracked_rigid_bodies, velocities, angulars);
      }

      if (this._received_configurations != null) {
        #if NEODROID_DEBUG
        if (this.Debugging) {
          Debug.Log($"Configuration length: {this._received_configurations.Length}");
        }
        #endif
        foreach (var configuration in this._received_configurations) {
          #if NEODROID_DEBUG
          if (this.Debugging) {
            Debug.Log("Configuring configurable with the specified name: " + configuration.ConfigurableName);
          }
          #endif
          if (this.Configurables.ContainsKey(configuration.ConfigurableName)) {
            this.Configurables[configuration.ConfigurableName].ApplyConfiguration(configuration);
          } else {
            #if NEODROID_DEBUG
            if (this.Debugging) {
              Debug.Log(
                  $"Could find not configurable with the specified name: {configuration.ConfigurableName}");
            }
            #endif
          }
        }
      } else {
        #if NEODROID_DEBUG
        if (this.Debugging) {
          Debug.Log("Has no received_configurations");
        }
        #endif
      }

      this.UpdateConfigurableValues();
      this.UpdateObserversData();
    }

    /// <summary>
    /// </summary>
    /// <param name="reaction"></param>
    void SendToDisplayers(Reaction reaction) {
      if (reaction.Displayables != null && reaction.Displayables.Length > 0) {
        foreach (var displayable in reaction.Displayables) {
          #if NEODROID_DEBUG
          if (this.Debugging) {
            Debug.Log("Applying " + displayable + " To " + this.name + "'s displayers");
          }
          #endif
          var displayable_name = displayable.DisplayableName;
          if (this.Displayers.ContainsKey(displayable_name) && this.Displayers[displayable_name] != null) {
            var v = displayable.DisplayableValue;
            this.Displayers[displayable_name].Display(v);
          } else {
            #if NEODROID_DEBUG
            if (this.Debugging) {
              Debug.Log("Could find not displayer with the specified name: " + displayable_name);
            }
            #endif
          }
        }
      }
    }

    /// <summary>
    /// </summary>
    /// <param name="reaction"></param>
    void SendToMotors(Reaction reaction) {
      if (reaction.Motions != null && reaction.Motions.Length > 0) {
        foreach (var motion in reaction.Motions) {
          #if NEODROID_DEBUG
          if (this.Debugging) {
            Debug.Log("Applying " + motion + " To " + this.name + "'s actors");
          }
          #endif
          var motion_actor_name = motion.ActorName;
          if (this.Actors.ContainsKey(motion_actor_name) && this.Actors[motion_actor_name] != null) {
            this.Actors[motion_actor_name].ApplyMotion(motion);
          } else {
            #if NEODROID_DEBUG
            if (this.Debugging) {
              Debug.Log("Could find not actor with the specified name: " + motion_actor_name);
            }
            #endif
          }
        }
      }
    }

    /// <summary>
    /// </summary>
    public event Action PreStepEvent;

    /// <summary>
    /// </summary>
    public event Action StepEvent;

    /// <summary>
    /// </summary>
    public event Action PostStepEvent;

    /// <summary>
    /// </summary>
    /// <param name="reaction"></param>
    void Step(Reaction reaction) {
      lock (this._reaction_lock) {
        this.PreStepEvent?.Invoke();

        if (reaction.Parameters.EpisodeCount) {
          this.CurrentFrameNumber++;
        } else {
          #if NEODROID_DEBUG
          if (this.Debugging) {
            Debug.LogWarning("Step did not count towards CurrentFrameNumber");
          }
          #endif
        }

        this.SendToMotors(reaction);

        this.StepEvent?.Invoke();

        if (this.EpisodeLength > 0 && this.CurrentFrameNumber >= this.EpisodeLength) {
          #if NEODROID_DEBUG
          if (this.Debugging) {
            Debug.Log($"Maximum episode length reached, Length {this.CurrentFrameNumber}");
          }
          #endif

          this.Terminate("Maximum episode length reached");
        }

        this.UpdateObserversData();
      }
    }

    /// <summary>
    /// </summary>
    protected void ResetRegisteredObjects() {
      #if NEODROID_DEBUG
      if (this.Debugging) {
        Debug.Log("Resetting registered objects");
      }
      #endif

      foreach (var configurable in this.Configurables.Values) {
        configurable?.EnvironmentReset();
      }

      foreach (var resetable in this.Resetables.Values) {
        resetable?.EnvironmentReset();
      }

      foreach (var actor in this.Actors.Values) {
        actor?.EnvironmentReset();
      }

      foreach (var observer in this.Observers.Values) {
        observer?.EnvironmentReset();
      }

      foreach (var listener in this.Listeners.Values) {
        listener?.EnvironmentReset();
      }
    }

    #region EnvironmentStateSetters

    /// <summary>
    /// </summary>
    /// <param name="child_game_objects"></param>
    /// <param name="positions"></param>
    /// <param name="rotations"></param>
    void SetEnvironmentPoses(GameObject[] child_game_objects, Vector3[] positions, Quaternion[] rotations) {
      if (this._Simulation_Manager != null) {
        for (var iterations = 1;
             iterations <= this._Simulation_Manager.SimulatorConfiguration.ResetIterations;
             iterations++) {
          for (var i = 0; i < child_game_objects.Length; i++) {
            if (child_game_objects[i] != null && i < positions.Length && i < rotations.Length) {
              var rigid_body = child_game_objects[i].GetComponent<Rigidbody>();
              if (rigid_body) {
                rigid_body.Sleep();
              }

              child_game_objects[i].transform.position = positions[i];
              child_game_objects[i].transform.rotation = rotations[i];
              if (rigid_body) {
                rigid_body.WakeUp();
              }

              var joint_fix = child_game_objects[i].GetComponent<JointFix>();
              if (joint_fix) {
                joint_fix.Reset();
              }

              var anim = child_game_objects[i].GetComponent<Animation>();
              if (anim) {
                anim.Rewind();
                anim.Play();
                anim.Sample();
                anim.Stop();
              }
            }
          }
        }
      }
    }

    /// <summary>
    /// </summary>
    /// <param name="bodies"></param>
    /// <param name="velocities"></param>
    /// <param name="angulars"></param>
    void SetEnvironmentBodies(Rigidbody[] bodies, Vector3[] velocities, Vector3[] angulars) {
      if (bodies != null && bodies.Length > 0) {
        for (var i = 0; i < bodies.Length; i++) {
          if (i < bodies.Length && bodies[i] != null && i < velocities.Length && i < angulars.Length) {
            #if NEODROID_DEBUG
            if (this.Debugging) {
              Debug.Log(
                  $"Setting {bodies[i].name}, velocity to {velocities[i]} and angular velocity to {angulars[i]}");
            }

            #endif

            bodies[i].Sleep();
            bodies[i].velocity = velocities[i];
            bodies[i].angularVelocity = angulars[i];
            bodies[i].WakeUp();
          }
        }
      }
    }

    #endregion

    #endregion
  }
}
