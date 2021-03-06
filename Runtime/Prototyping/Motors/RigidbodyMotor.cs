﻿using Neodroid.Runtime.Interfaces;
using Neodroid.Runtime.Prototyping.Actors;
using Neodroid.Runtime.Utilities.Misc;
using UnityEngine;

namespace Neodroid.Runtime.Prototyping.Motors {
  /// <inheritdoc />
  /// <summary>
  /// </summary>
  [AddComponentMenu(
      MotorComponentMenuPath._ComponentMenuPath + "Rigidbody" + MotorComponentMenuPath._Postfix)]
  [RequireComponent(typeof(Rigidbody))]
  public class RigidbodyMotor : Motor {
    /// <summary>
    /// </summary>
    [SerializeField]
    protected ForceMode _ForceMode = ForceMode.Force;

    /// <summary>
    /// </summary>
    [SerializeField]
    protected Space _Relative_To = Space.Self;

    /// <summary>
    /// </summary>
    [SerializeField]
    protected Rigidbody _Rigidbody;

    string _rot_x;
    string _rot_y;
    string _rot_z;

    string _x;
    string _y;
    string _z;

    public override string PrototypingTypeName { get { return "Rigidbody"; } }

    /// <inheritdoc />
    /// <summary>
    /// </summary>
    protected override void Setup() {
      this._Rigidbody = this.GetComponent<Rigidbody>();

      this._x = this.Identifier + "X_";
      this._y = this.Identifier + "Y_";
      this._z = this.Identifier + "Z_";
      this._rot_x = this.Identifier + "RotX_";
      this._rot_y = this.Identifier + "RotY_";
      this._rot_z = this.Identifier + "RotZ_";
    }

    /// <summary>
    /// </summary>
    protected override void RegisterComponent() {
      this.ParentActor = NeodroidUtilities.RegisterComponent((Actor)this.ParentActor, (Motor)this);

      this.ParentActor = NeodroidUtilities.RegisterComponent((Actor)this.ParentActor, (Motor)this, this._x);
      this.ParentActor = NeodroidUtilities.RegisterComponent((Actor)this.ParentActor, (Motor)this, this._y);
      this.ParentActor = NeodroidUtilities.RegisterComponent((Actor)this.ParentActor, (Motor)this, this._z);
      this.ParentActor = NeodroidUtilities.RegisterComponent(
          (Actor)this.ParentActor,
          (Motor)this,
          this._rot_x);
      this.ParentActor = NeodroidUtilities.RegisterComponent(
          (Actor)this.ParentActor,
          (Motor)this,
          this._rot_y);
      this.ParentActor = NeodroidUtilities.RegisterComponent(
          (Actor)this.ParentActor,
          (Motor)this,
          this._rot_z);
    }

    /// <summary>
    /// </summary>
    /// <param name="motion"></param>
    protected override void InnerApplyMotion(IMotorMotion motion) {
      if (this._Relative_To == Space.World) {
        if (motion.MotorName == this._x) {
          this._Rigidbody.AddForce(Vector3.left * motion.Strength, this._ForceMode);
        } else if (motion.MotorName == this._y) {
          this._Rigidbody.AddForce(Vector3.up * motion.Strength, this._ForceMode);
        } else if (motion.MotorName == this._z) {
          this._Rigidbody.AddForce(Vector3.forward * motion.Strength, this._ForceMode);
        } else if (motion.MotorName == this._rot_x) {
          this._Rigidbody.AddTorque(Vector3.left * motion.Strength, this._ForceMode);
        } else if (motion.MotorName == this._rot_y) {
          this._Rigidbody.AddTorque(Vector3.up * motion.Strength, this._ForceMode);
        } else if (motion.MotorName == this._rot_z) {
          this._Rigidbody.AddTorque(Vector3.forward * motion.Strength, this._ForceMode);
        }
      } else if (this._Relative_To == Space.Self) {
        if (motion.MotorName == this._x) {
          this._Rigidbody.AddRelativeForce(Vector3.left * motion.Strength, this._ForceMode);
        } else if (motion.MotorName == this._y) {
          this._Rigidbody.AddRelativeForce(Vector3.up * motion.Strength, this._ForceMode);
        } else if (motion.MotorName == this._z) {
          this._Rigidbody.AddRelativeForce(Vector3.forward * motion.Strength, this._ForceMode);
        } else if (motion.MotorName == this._rot_x) {
          this._Rigidbody.AddRelativeTorque(Vector3.left * motion.Strength, this._ForceMode);
        } else if (motion.MotorName == this._rot_y) {
          this._Rigidbody.AddRelativeTorque(Vector3.up * motion.Strength, this._ForceMode);
        } else if (motion.MotorName == this._rot_z) {
          this._Rigidbody.AddRelativeTorque(Vector3.forward * motion.Strength, this._ForceMode);
        }
      } else {
        Debug.LogWarning($"Not applying force in space {this._Relative_To}");
      }
    }
  }
}