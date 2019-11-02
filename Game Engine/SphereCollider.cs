using Microsoft.Xna.Framework;

namespace CPI311.GameEngine
{
    public class SphereCollider : Collider
    {

        public float Radius { get; set; }

        public override bool Collides(Collider other, out Vector3 normal)
        {
            if (other is SphereCollider)
            {
                SphereCollider collider = other as SphereCollider;
                if ((Transform.Position - collider.Transform.Position).LengthSquared() <
                    System.Math.Pow(Radius + collider.Radius, 2))
                {
                    //System.Console.WriteLine("Collided");
                    normal = Vector3.Normalize(Transform.Position - collider.Transform.Position);
                    return true;
                }
            }
            else if (other is BoxCollider)
                return other.Collides(this, out normal);
            return base.Collides(other, out normal);
        }

        // *** Lab 7: Solution of SweptCollides
        public bool SweptCollides(Collider other, Vector3 otherLastPosition, Vector3 lastPosition, out Vector3 normal)
        {
            if (other is SphereCollider)
            {
                SphereCollider collider = other as SphereCollider;
                Vector3 vp = Transform.Position - lastPosition;
                Vector3 vq = collider.Transform.Position - otherLastPosition;
                Vector3 A = Transform.Position - collider.Transform.Position;
                Vector3 B = vp - vq;
                float a = Vector3.Dot(B, B);
                float b = 2 * Vector3.Dot(A, B);
                float c = Vector3.Dot(A, A) - ((collider.Radius + this.Radius) * (collider.Radius + this.Radius));
                float disc = b * b - 4 * a * c;
                if (disc >= 0)
                {
                    float t = (-b - (float)System.Math.Sqrt(disc)) / (2 * a);
                    Vector3 p = lastPosition + t * vp;
                    Vector3 q = otherLastPosition + t * vq;
                    Vector3 intersect = Vector3.Lerp(p, q, this.Radius / (this.Radius + collider.Radius));
                    normal = Vector3.Normalize(p - q);
                    return true;
                }
            }
            else if (other is BoxCollider)
                return other.Collides(this, out normal);
            return base.Collides(other, out normal);
        }

        // Lab 8 or later *****************************************
        public override float? Intersects(Ray ray)
        {
            Matrix worldInverted = Matrix.Invert(Transform.World);
            ray.Position = Vector3.Transform(ray.Position, worldInverted);
            ray.Direction = Vector3.Normalize(Vector3.TransformNormal(ray.Direction, worldInverted));
            BoundingSphere sphere = new BoundingSphere(Vector3.Zero, Radius);
            return sphere.Intersects(ray);
        }
        //**********************************************************

       
       
    }
}
