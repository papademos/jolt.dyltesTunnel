using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Jolt.Math;

// Misc math helper methods, copied from one of my private repos.
// Note: Angles are expressed in Revolutions. (1 Revolution = 360 degrees = 2*PI).
static class MathF
{
    //
    public const float PI = (float)System.Math.PI;
    public const float Epsilon = 1e-6f;

    //
    public static bool IsZero(float x) => Abs(x) < Epsilon;
    public static int Sign(float x) => IsZero(x) ? 0 : System.Math.Sign(x);

    //
    public static int Mod(int x, int y) => (x % y + y) % y;
    public static float Abs(float x) => x < 0 ? -x : x;
    public static float Pow(float x, float y) => (float)System.Math.Pow(x, y);
    public static float Sqrt(float x) => (float)System.Math.Sqrt(x);

    //
    public static int Min(int x, int y) => x < y ? x : y;
    public static int Max(int x, int y) => x > y ? x : y;
    public static long Min(long x, long y) => x < y ? x : y;
    public static long Max(long x, long y) => x > y ? x : y;
    public static float Min(float x, float y) => x < y ? x : y;
    public static float Max(float x, float y) => x > y ? x : y;
    public static float Clamp(float x, float min, float max) => x < min ? min : x < max ? x : max;
    public static float Saturate(float x) => Clamp(x, 0, 1);

    //
    public static float RadToRev(float rad) => rad / 2 / (float)System.Math.PI;
    public static float RevToRad(float rev) => rev * 2 * (float)System.Math.PI;

    //
    public static float Sin(float a) => (float)System.Math.Sin(RevToRad(a));
    public static float Cos(float a) => (float)System.Math.Cos(RevToRad(a));
    public static float Asin(float value) => RadToRev((float)System.Math.Asin(value));
    public static float Acos(float value) => RadToRev((float)System.Math.Acos(value));
    public static float Atan(float value) => RadToRev((float)System.Math.Atan(value));
    public static float Atan(float y, float x) => RadToRev((float)System.Math.Atan2(y, x));

    //
    public static float Lerp(float min, float max, float t) => min + t * (max - min);
    public static float Angle(Vector3 u, Vector3 v) => Acos(Vector3.Dot(u, v));
    public static float Cross(Vector2 u, Vector2 v) => u.X * v.Y - u.Y * v.X; // Note: This is basically the Z result of the 3D cross product.

    //
    public static Matrix4x4 RotX(float a) => Matrix4x4.CreateRotationX(RevToRad(a));
    public static Matrix4x4 RotY(float a) => Matrix4x4.CreateRotationY(RevToRad(a));
    public static Matrix4x4 RotZ(float a) => Matrix4x4.CreateRotationZ(RevToRad(a));
    public static Matrix4x4 Translate(float x, float y, float z) => Matrix4x4.CreateTranslation(x, y, z);
    public static Matrix4x4 Perspective(float w, float h, float near, float far) => Matrix4x4.CreatePerspective(w, h, near, far);

    //
    public static Vector2 V(float x, float y) => new(x, y);
    public static Vector3 V(float x, float y, float z) => new(x, y, z);
    public static Vector4 V(float x, float y, float z, float w) => new(x, y, z, w);
    public static Vector2 V2(Vector3 v3) => new(v3.X, v3.Y);
    public static Vector2 V2(Vector4 v4) => new(v4.X, v4.Y);
    public static Vector3 V3(Vector2 v2) => new(v2.X, v2.Y, 0);
    public static Vector3 V3(Vector4 v4) => new(v4.X, v4.Y, v4.Z);
    public static Vector4 V4(Vector3 v3, float w) => new(v3, w);
    public static Vector4 V4(Vector2 v2) => new(v2.X, v2.Y, 0, 0);

    //
    public static IEnumerable<Vector4> Transform(this IEnumerable<Vector4> positions, Matrix4x4 m)
        => positions.Select(p => Vector4.Transform(p, m));
}
