﻿using System;
using UnityEngine;

namespace Neodroid.Utilities.ScriptableObjects {
  /// <summary>
  /// Determines the discrete timesteps of the simulation environment.
  /// </summary>
  public enum SimulationType {
    /// <summary>
    /// Waiting for frame instead means stable physics(Multiple fixed updates) and camera has updated their rendertextures. Pauses the game after every reaction until next reaction is received.
    /// </summary>
    Frame_dependent_,

    /// <summary>
    /// Camera observers should be manually rendered to ensure validity and freshness with camera.Render()
    /// </summary>
    Physics_dependent_,

    /// <summary>
    ///  Continue simulation
    /// </summary>
    Independent_

  }

  /// <summary>
  /// Determines where in the monobehaviour cycle a frame/step is finished
  /// </summary>
  public enum FrameFinishes {
    /// <summary>
    /// When ever all scripts has run their respective updates
    /// </summary>
    Late_update_,
    /// <summary>
    /// 
    /// </summary>
    On_render_image_,
    /// <summary>
    /// When ever a
    /// </summary>
    On_post_render_,
    /// <summary>
    /// 
    /// </summary>
    End_of_frame_
  }

  /// <inheritdoc />
  /// <summary>
  /// Contains everything relevant to configuring simulation environments engine specific settings 
  /// </summary>
  [CreateAssetMenu(
      fileName = "SimulatorConfiguration",
      menuName = "Neodroid/ScriptableObjects/SimulatorConfiguration",
      order = 1)]
  [Serializable]
  public class SimulatorConfiguration : ScriptableObject {

    
    [Header("Graphics")]
    [SerializeField] bool _full_screen = false;
    [SerializeField] [Range(0, 9999)] int _height =500;
    [SerializeField] [Range(0, 9999)] int _width =500;

    [SerializeField] [Range(1, 4)] int _quality_level = 1;

    

    [Header("Simulation")]
    [SerializeField] FrameFinishes _frame_finishes=FrameFinishes.Late_update_;
    [SerializeField] [Range(0, 99)] int _frame_skips=0;
    [SerializeField] SimulationType _simulation_type=SimulationType.Frame_dependent_;
    [SerializeField] [Range(-1, 9999)] int _target_frame_rate=-1;
    [SerializeField] [Range(0f, float.MaxValue)] float _time_scale=1;
    [SerializeField] [Range(1, 99)] int _reset_iterations = 10;
    [SerializeField] [Range(0, 9999)] float _max_reply_interval = 0;
    
    public void SetToDefault() {
      this.Width = 500;
      this.Height = 500;
      this.FullScreen = false;
      this.QualityLevel = 1;
      this.TimeScale = 1;
      this.TargetFrameRate = -1;
      this.SimulationType = SimulationType.Frame_dependent_;
      this.FrameFinishes = FrameFinishes.Late_update_;
      this.FrameSkips = 0;
      this.ResetIterations = 10;
      this.MaxReplyInterval = 0;
    }

    #region Getter Setters

    /// <summary>
    /// 
    /// </summary>
    public int FrameSkips {
      get { return this._frame_skips; }
      set {
        if (value >= 0)
          this._frame_skips = value;
      }
    }

    /// <summary>
    /// 
    /// </summary>
    public int ResetIterations {
      get { return this._reset_iterations; }
      set {
        if (value >= 1)
          this._reset_iterations = value;
      }
    }
    //When resetting transforms we run multiple times to ensure that we properly reset hierachies of objects

    /// <summary>
    /// 
    /// </summary>
    public SimulationType SimulationType {
      get { return this._simulation_type; }
      set { this._simulation_type = value; }
    }

    /// <summary>
    /// 
    /// </summary>
    public int Width {
      get { return this._width; }
      set {
        if (value >= 0)
          this._width = value;
      }
    }

    /// <summary>
    /// 
    /// </summary>
    public int Height {
      get { return this._height; }
      set {
        if (value >= 0)
          this._height = value;
      }
    }

    /// <summary>
    /// 
    /// </summary>
    public bool FullScreen { get { return this._full_screen; } set { this._full_screen = value; } }

    /// <summary>
    /// 
    /// </summary>
    public int TargetFrameRate {
      get { return this._target_frame_rate; }
      set {
        if (value >= -1)
          this._target_frame_rate = value;
      }
    }

    /// <summary>
    /// 
    /// </summary>
    public int QualityLevel {
      get { return this._quality_level; }
      set {
        if (value >= 1 && value <= 4)
          this._quality_level = value;
      }
    }

    /// <summary>
    /// 
    /// </summary>
    public float TimeScale {
      get { return this._time_scale; }
      set {
        if (value >= 0)
          this._time_scale = value;
      }
    }

    /// <summary>
    /// 
    /// </summary>
    public Single MaxReplyInterval {
      get { return this._max_reply_interval; }
      set { this._max_reply_interval = value; }
    }

    /// <summary>
    /// 
    /// </summary>
    public FrameFinishes FrameFinishes {
      get { return this._frame_finishes; }
      set { this._frame_finishes = value; }
    }

    #endregion
  }
}
