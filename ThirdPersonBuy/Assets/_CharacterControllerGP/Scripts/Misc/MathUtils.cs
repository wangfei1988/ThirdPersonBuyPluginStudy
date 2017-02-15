// © 2015 Mario Lelas
using UnityEngine;

namespace MLSpace
{
    /// <summary>
    /// some math utils
    /// </summary>
    public static class MathUtils
    {
        /// <summary>
        /// keep angle in -180 to 180 interval
        /// </summary>
        /// <param name="angle"></param>
        /// <returns></returns>
        public static float WrapAngle180(float angle)
        {
            float newAngle = angle;
            if (angle > 179)
                newAngle = -180;
            if (angle < -179)
                newAngle = 180;
            return newAngle;
        }

        /// <summary>
        /// keep angle in -360 to 360 interval
        /// </summary>
        /// <param name="angle"></param>
        /// <returns></returns>
        public static float WrapAngle360(float angle)
        {
            float newAngle = angle;
            if (angle > 359)
                newAngle = -360;
            if (angle < -359)
                newAngle = 360;
            return newAngle;
        }

        /// <summary>
        /// plane dot coodinate function
        /// </summary>
        /// <param name="plane"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static float PlaneDotCoordinate(ref Plane plane, ref Vector3 value)
        {
            float num1 = ((value.x * plane.normal.x) + (value.y * plane.normal.y)) +
                 (value.z * plane.normal.z);
            float num2 = num1 + plane.distance;
            return num2;
        }
        
        /// <summary>
        /// test intersection sphere with frustum
        /// </summary>
        /// <param name="planes">plane array</param>
        /// <param name="center">center of sphere</param>
        /// <param name="radius">radius of sphere</param>
        /// <returns>true or false</returns>
        public static bool TestSphereFrustum(Plane[] planes,ref Vector3 center, float radius)  
	    {
            for (int i = 0; i < 6; ++i)
            {
                float dotCoord = PlaneDotCoordinate(ref planes[i], ref  center) + radius;
                if (dotCoord < 0.0f)
                    return false;
            }
            return true;
	    }

        /// <summary>
        /// get closest point to line ( duh )
        /// </summary>
        /// <param name="A">line start</param>
        /// <param name="B">line end</param>
        /// <param name="P">point to test ( reference )</param>
        /// <returns>closest point</returns>
        public static Vector3 GetClosestPoint2Line(Vector3 A, Vector3 B, ref Vector3 P)
        {
            Vector3 AP = P - A;       //Vector from A to P   
            Vector3 AB = B - A;       //Vector from A to B  

            float magnitudeAB = Vector3.SqrMagnitude(AB);// //Magnitude of AB vector (it's length squared)     
            float ABAPproduct = Vector3.Dot(AP, AB);    //The DOT product of a_to_p and a_to_b     
            float distance = ABAPproduct / magnitudeAB; //The normalized "distance" from a to your closest point  

            if (distance < 0)     //Check if P projection is over vectorAB     
            {
                return A;

            }
            else if (distance > 1)
            {
                return B;
            }
            else
            {
                return A + AB * distance;
            }
        }

        /// <summary>
        /// Check if point is between line start and end.
        /// Point does not have to be on line
        /// </summary>
        /// <param name="lineA">line start</param>
        /// <param name="lineB">line end</param>
        /// <param name="point">point to test ( reference )</param>
        /// <returns>true or false</returns>
        public static bool IsInsideLineSegment(Vector3 lineA, Vector3 lineB, ref Vector3 point)
        {
            Vector3 AP = point - lineA;
            Vector3 AB = lineB - lineA;

            float magnitudeAB = Vector3.SqrMagnitude(AB);
            float ABAPproduct = Vector3.Dot(AP, AB);
            float distance = ABAPproduct / magnitudeAB;

            if (distance < 0)
            {
                return false;

            }
            else if (distance > 1)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// calculates closest point to line and 
        /// assignes distances from two edges to that closest point
        /// /// </summary>
        /// <param name="A">first point of line</param>
        /// <param name="B">second point of line</param>
        /// <param name="P">point in space to test</param>
        /// <param name="distFromA">assignes distance from point a</param>
        /// <param name="distFromB">assignes distnace from point b</param>
        public static void GetLineEdgeDistances(Vector3 A, Vector3 B, Vector3 P, out float distFromA, out float distFromB)
        {
            Vector3 AP = P - A;     
            Vector3 AB = B - A;   

            float magnitudeAB = Vector3.SqrMagnitude(AB);   
            float ABAPproduct = Vector3.Dot(AP, AB);   
            float distance = ABAPproduct / magnitudeAB; 

            Vector3 closest = A;
            distFromA = 0.0f;
            distFromB = AB.magnitude;
            if (distance > 1)
            {
                closest = B;
                distFromA = AB.magnitude;
                distFromB = 0.0f;
            }
            else
            {
                closest = A + AB * distance;
                distFromA = Vector3.Distance(closest, A);
                distFromB = Vector3.Distance(closest, B);
            }
        }

        /// <summary>
        /// get closest point to plane
        /// </summary>
        /// <param name="plane">plane reference</param>
        /// <param name="inpos">point reference</param>
        /// <returns>closest point</returns>
        public static Vector3 GetClosestPoint2Plane(ref Plane plane,ref Vector3 inpos)
        {
            float dist2Plane = plane.GetDistanceToPoint(inpos);
            Vector3 normal = plane.normal;
            return inpos - normal * dist2Plane;
        }

    }


}
