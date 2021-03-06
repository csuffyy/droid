using System.Collections.Generic;
using Neodroid.Runtime.Utilities.Misc.SearchableEnum;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

#endif

namespace Neodroid.Runtime.Utilities.BoundingBoxes {
  public enum BasedOn {
    Geometry_,
    Collider_
  }

  /// <inheritdoc />
  /// <summary>
  /// </summary>
  [ExecuteInEditMode]
  public class BoundingBox : MonoBehaviour {
    /// <summary>
    /// </summary>
    Vector3 _bottom_back_left;

    /// <summary>
    /// </summary>
    Vector3 _bottom_back_right;

    /// <summary>
    /// </summary>
    Vector3 _bottom_front_left;

    /// <summary>
    /// </summary>
    Vector3 _bottom_front_right;

    /// <summary>
    /// </summary>
    protected Bounds _Bounds;

    /// <summary>
    /// </summary>
    protected Vector3 _Bounds_Offset;

    /// <summary>
    /// </summary>
    Collider[] _children_colliders;

    /// <summary>
    /// </summary>
    MeshFilter[] _children_meshes;

    /// <summary>
    /// </summary>
    [SearchableEnum]
    public BasedOn _BasedOn;

    /// <summary>
    /// </summary>
    Vector3[] _points;

    /// <summary>
    /// </summary>
    public bool _FreezeAfterFirstCalculation;

    /// <summary>
    /// </summary>
    public bool _Include_Children = true;

    Vector3 _last_position;
    Quaternion _last_rotation;

    /// <summary>
    /// </summary>
    Vector3 _last_scale;

    /// <summary>
    /// </summary>
    public Color _Line_Color = new Color(1f, 0.36f, 0.38f, 0.74f);

    /// <summary>
    /// </summary>
    Vector3[,] _lines;

    List<Vector3[]> _lines_list = new List<Vector3[]>();

    /// <summary>
    /// </summary>
    Quaternion _rotation;

    /// <summary>
    /// </summary>
    public bool _Setup_On_Awake;

    Vector3 _top_back_left;
    Vector3 _top_back_right;

    Vector3 _top_front_left;
    Vector3 _top_front_right;

    public Vector3[] BoundingBoxCoordinates {
      get {
        return new[] {
            this._top_front_left,
            this._top_front_right,
            this._top_back_left,
            this._top_back_right,
            this._bottom_front_left,
            this._bottom_front_right,
            this._bottom_back_left,
            this._bottom_back_right
        };
      }
    }

    public Bounds Bounds { get { return this._Bounds; } }

    public Vector3 Max { get { return this._Bounds.max; } }

    public Vector3 Min { get { return this._Bounds.min; } }

    /// <summary>
    /// </summary>
    public string BoundingBoxCoordinatesAsString {
      get {
        var str_rep = "";
        str_rep += "\"_top_front_left\":" + this.BoundingBoxCoordinates[0] + ", ";
        str_rep += "\"_top_front_right\":" + this.BoundingBoxCoordinates[1] + ", ";
        str_rep += "\"_top_back_left\":" + this.BoundingBoxCoordinates[2] + ", ";
        str_rep += "\"_top_back_right\":" + this.BoundingBoxCoordinates[3] + ", ";
        str_rep += "\"_bottom_front_left\":" + this.BoundingBoxCoordinates[4] + ", ";
        str_rep += "\"_bottom_front_right\":" + this.BoundingBoxCoordinates[5] + ", ";
        str_rep += "\"_bottom_back_left\":" + this.BoundingBoxCoordinates[6] + ", ";
        str_rep += "\"_bottom_back_right\":" + this.BoundingBoxCoordinates[7];
        return str_rep;
      }
    }

    /// <summary>
    /// </summary>
    public string BoundingBoxCoordinatesAsJson {
      get {
        var str_rep = "{";
        str_rep += "\"_top_front_left\":" + this.JsonifyVec3(this.BoundingBoxCoordinates[0]) + ", ";
        str_rep += "\"_bottom_back_right\":" + this.JsonifyVec3(this.BoundingBoxCoordinates[7]);
        str_rep += "}";
        return str_rep;
      }
    }

    /// <summary>
    /// </summary>
    public Vector3[,] Lines { get { return this._lines; } set { this._lines = value; } }

    /// <summary>
    /// </summary>
    public Vector3[] Points { get { return this._points; } set { this._points = value; } }

    /// <summary>
    /// </summary>
    /// <param name="vec"></param>
    /// <returns></returns>
    string JsonifyVec3(Vector3 vec) { return $"[{vec.x},{vec.y},{vec.z}]"; }

    /// <summary>
    /// </summary>
    void Reset() {
      this.Awake();
      this.Start();
    }

    /// <summary>
    /// </summary>
    void Start() {
      if (!this._Setup_On_Awake) {
        this.Setup();
      }
    }

    /// <summary>
    /// </summary>
    void Awake() {
      if (this._Setup_On_Awake) {
        this.Setup();
      }
    }

    /// <summary>
    /// </summary>
    void Setup() {
      this._last_position = this.transform.position;
      this._last_rotation = this.transform.rotation;
      this._last_scale = this.transform.localScale;

      this._children_meshes = this.GetComponentsInChildren<MeshFilter>();
      this._children_colliders = this.GetComponentsInChildren<Collider>();

      this.CalculateBounds();
      this.Initialise();
    }

    /// <summary>
    /// </summary>
    public void Initialise() {
      this.RecalculatePoints();
      this.RecalculateLines();
    }

    /// <summary>
    /// </summary>
    void Update() {
      if (this._FreezeAfterFirstCalculation) {
        return;
      }

      if (this._children_meshes != this.GetComponentsInChildren<MeshFilter>()) {
        this.Reset();
      }

      if (this._children_colliders != this.GetComponentsInChildren<Collider>()) {
        this.Reset();
      }

      if (this.transform.localScale != this._last_scale) {
        this.ScaleBounds();
        this.RecalculatePoints();
      }

      if (this.transform.position != this._last_position
          || this.transform.rotation != this._last_rotation
          || this.transform.localScale != this._last_scale) {
        this.RecalculateLines();
        this._last_rotation = this.transform.rotation;
        this._last_position = this.transform.position;
        this._last_scale = this.transform.localScale;
      }
    }

    /// <summary>
    /// </summary>
    public void ScaleBounds() {
      //this._Bounds.size = new Vector3(startingBoundSize.x * transform.localScale.x / startingScale.x, startingBoundSize.y * transform.localScale.y / startingScale.y, startingBoundSize.z * transform.localScale.z / startingScale.z);
      //this._Bounds.center = transform.TransformPoint(startingBoundCenterLocal);
    }

    /// <summary>
    /// </summary>
    void FitBoundingBoxToChildrenColliders() {
      var col = this.GetComponent<BoxCollider>();
      var bounds = new Bounds(this.transform.position, Vector3.zero); // position and size

      if (col) {
        bounds.Encapsulate(col.bounds);
      }

      if (this._Include_Children) {
        foreach (var child_col in this._children_colliders) {
          if (child_col != col) {
            bounds.Encapsulate(child_col.bounds);
          }
        }
      }

      this._Bounds = bounds;
      this._Bounds_Offset = bounds.center - this.transform.position;
    }

    /// <summary>
    /// </summary>
    void FitBoundingBoxToChildrenRenders() {
      var bounds = new Bounds(this.transform.position, Vector3.zero);

      var mesh = this.GetComponent<MeshFilter>();
      if (mesh) {
        var ms = mesh.sharedMesh;
        var vc = ms.vertexCount;
        for (var i = 0; i < vc; i++) {
          bounds.Encapsulate(mesh.transform.TransformPoint(ms.vertices[i]));
        }
      }

      foreach (var t in this._children_meshes) {
        if (t) {
          var ms = t.sharedMesh;
          if (ms) {
            var vc = ms.vertexCount;
            for (var j = 0; j < vc; j++) {
              bounds.Encapsulate(t.transform.TransformPoint(ms.vertices[j]));
            }
          }
        }
      }

      this._Bounds = bounds;
      this._Bounds_Offset = this._Bounds.center - this.transform.position;
    }

    /// <summary>
    /// </summary>
    void CalculateBounds() {
      this._rotation = this.transform.rotation;
      this.transform.rotation = Quaternion.Euler(0f, 0f, 0f);

      if (this._BasedOn == BasedOn.Collider_) {
        this.FitBoundingBoxToChildrenColliders();
      } else {
        this.FitBoundingBoxToChildrenRenders();
      }

      this.transform.rotation = this._rotation;
    }

    /// <summary>
    /// </summary>
    void RecalculatePoints() {
      this._Bounds.size = new Vector3(
          this._Bounds.size.x * this.transform.localScale.x / this._last_scale.x,
          this._Bounds.size.y * this.transform.localScale.y / this._last_scale.y,
          this._Bounds.size.z * this.transform.localScale.z / this._last_scale.z);
      this._Bounds_Offset = new Vector3(
          this._Bounds_Offset.x * this.transform.localScale.x / this._last_scale.x,
          this._Bounds_Offset.y * this.transform.localScale.y / this._last_scale.y,
          this._Bounds_Offset.z * this.transform.localScale.z / this._last_scale.z);

      this._top_front_right = this._Bounds_Offset + Vector3.Scale(this._Bounds.extents, new Vector3(1, 1, 1));
      this._top_front_left = this._Bounds_Offset + Vector3.Scale(this._Bounds.extents, new Vector3(-1, 1, 1));
      this._top_back_left = this._Bounds_Offset + Vector3.Scale(this._Bounds.extents, new Vector3(-1, 1, -1));
      this._top_back_right = this._Bounds_Offset + Vector3.Scale(this._Bounds.extents, new Vector3(1, 1, -1));
      this._bottom_front_right =
          this._Bounds_Offset + Vector3.Scale(this._Bounds.extents, new Vector3(1, -1, 1));
      this._bottom_front_left =
          this._Bounds_Offset + Vector3.Scale(this._Bounds.extents, new Vector3(-1, -1, 1));
      this._bottom_back_left =
          this._Bounds_Offset + Vector3.Scale(this._Bounds.extents, new Vector3(-1, -1, -1));
      this._bottom_back_right =
          this._Bounds_Offset + Vector3.Scale(this._Bounds.extents, new Vector3(1, -1, -1));

      this.Points = new[] {
          this._top_front_right,
          this._top_front_left,
          this._top_back_left,
          this._top_back_right,
          this._bottom_front_right,
          this._bottom_front_left,
          this._bottom_back_left,
          this._bottom_back_right
      };
    }

    /// <summary>
    /// </summary>
    void RecalculateLines() {
      var rot = this.transform.rotation;
      var pos = this.transform.position;

      this._lines_list.Clear();
      //int linesCount = 12;

      for (var i = 0; i < 4; i++) {
        //width
        var line = new[] {rot * this.Points[2 * i] + pos, rot * this.Points[2 * i + 1] + pos};
        this._lines_list.Add(line);

        //height
        line = new[] {rot * this.Points[i] + pos, rot * this.Points[i + 4] + pos};
        this._lines_list.Add(line);

        //depth
        line = new[] {rot * this.Points[2 * i] + pos, rot * this.Points[2 * i + 3 - 4 * (i % 2)] + pos};
        this._lines_list.Add(line);
      }

      this.Lines = new Vector3[this._lines_list.Count, 2];
      for (var j = 0; j < this._lines_list.Count; j++) {
        this.Lines[j, 0] = this._lines_list[j][0];
        this.Lines[j, 1] = this._lines_list[j][1];
      }
    }

    /// <summary>
    /// </summary>
    void OnMouseDown() {
      //if (_permanent)
      //  return;
      this.enabled = !this.enabled;
    }

    #if UNITY_EDITOR
    /// <summary>
    /// </summary>
    void OnValidate() {
      if (EditorApplication.isPlaying) {
        return;
      }

      this.Initialise();
    }

    /// <summary>
    /// </summary>
    void OnDrawGizmos() {
      if (this.enabled) {
        if (this.enabled) {
          Gizmos.color = this._Line_Color;
          if (this.Lines != null) {
            for (var i = 0; i < this.Lines.GetLength(0); i++) {
              Gizmos.DrawLine(this.Lines[i, 0], this.Lines[i, 1]);
            }
          }

          Handles.Label(this.transform.position, this.transform.gameObject.name);
        }
      }
    }
    #endif
  }
}