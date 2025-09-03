using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClassicMask : MaskGenerator
{
    public ClassicMask(Vector2Int res, int num, MaskType mt)
      : base(res, num, mt) { }

    protected override void CreatePuzzleMask(Texture2D c, MaskType t)
    {
        DrawClassicParLines(c, pixel_sides.x, pixel_sides.y, c.width, number_of_puzzles.y);
        DrawClassicParLines(c, pixel_sides.y, pixel_sides.x, c.height, number_of_puzzles.x, true);
    }
    private void DrawClassicParLines(Texture2D c, float side_x, float side_y, int width, float numb_of_puz, bool vert = false)
    {
        float a = side_x / 6;
        float b = side_y / 8;

        float sin_coef = 2;
        var x0 = side_x / 2 - a;
        float v = (x0 + side_x) * Mathf.PI / side_x;
        float f_x0 = sin_coef * Mathf.Sin(v) + side_y;


        //drawing sin lines
        for (int j = 1; j < numb_of_puz; j++)
        {
            for (int i = 0; i < width; i++)
            {
                if (i % side_x - x0 <= 0 || i % side_x - (side_x / 2 - x0 + side_x / 2) >= 0)
                {
                    int s = Mathf.RoundToInt(sin_coef * Mathf.Sin((j % 2 == 0 ? i : i + side_x) * Mathf.PI / side_x) + j * side_y);
                    SetPixelsInRange(c, i, s, Color.black, vert);
                }
            }
        }

        Vector2[] centers_for_tangents = new Vector2[3] { Vector2.zero, Vector2.zero, Vector2.zero };
        centers_for_tangents[1] = new Vector2(x0, f_x0);
        float tan1 = 0, tan2 = 0, tan_sin = 0;

        for (int j = 1; j < numb_of_puz; j++)
        {
            for (float i = side_x / 2; i < width; i += side_x)
            {
                float rand = Random.Range(0f, 1f);
                float sin = sin_coef * Mathf.Sin((j % 2 == 0 ? i : i + side_x) * Mathf.PI / side_x);

                float con_shift = (rand > 0.5f ? sin + b : sin - b);
                bool is_bottom = rand <= 0.5f;
                con_shift *= 1.1f;

                int normal_index = is_bottom ? 2 : 0;

                if (j == 1 && i < side_x)
                {
                    CalculateTangents(rand, sin, is_bottom, ref tan1, ref tan2, ref tan_sin, ref centers_for_tangents);
                }

                // drawing ellipses
                for (float t = 0; t <= Mathf.PI * 2; t += Mathf.PI / 720)
                {
                    float ang = ellipse_angle(a, b, t);
                    if (is_bottom)  //if under the sin line
                    {
                        if (t < -Mathf.Atan(tan1) || t > Mathf.Atan(tan1) + Mathf.PI)
                        {

                            SetPixelsInRange(c, Mathf.RoundToInt(i + a * Mathf.Cos(ang)), Mathf.RoundToInt(j * side_y + con_shift + b * Mathf.Sin(ang)), Color.black, vert);

                        }
                    }
                    else            //if over the sin line
                    {
                        if (t < Mathf.Atan(tan2) + Mathf.PI || t > 2 * Mathf.PI - Mathf.Atan(tan2))
                        {
                            SetPixelsInRange(c, Mathf.RoundToInt(i + a * Mathf.Cos(ang)), Mathf.RoundToInt(j * side_y + con_shift + b * Mathf.Sin(ang)), Color.black, vert);
                        }
                    }
                }

                // drawing circles (conjugation) of ellipse and sin lines
                for (float t = 0; t <= Mathf.PI * 2; t += Mathf.PI / 360)
                {
                    float d;

                    if (sin < 0) // if ellipse nierby lowest point of sin line
                    {
                        //getting distance
                        d = Vector2.Distance(centers_for_tangents[1], centers_for_tangents[normal_index]);

                        // left circle                      
                        if (is_bottom ? (t <= tan_sin || t >= 2 * Mathf.PI + tan1) : (t <= tan2 || t >= tan_sin + Mathf.PI))
                            SetPixelsInRange(c, Mathf.RoundToInt(i - (side_x / 2) + centers_for_tangents[normal_index].x + d * Mathf.Cos(t)),
                                   Mathf.RoundToInt(j * side_y - side_y + centers_for_tangents[normal_index].y + d * Mathf.Sin(t)),
                                   Color.black, vert);

                        // right circle
                        if (is_bottom ? (t >= Mathf.PI - tan_sin && t <= Mathf.PI - tan1) : (t >= Mathf.PI - tan2 && t <= 2 * Mathf.PI - tan_sin))
                            SetPixelsInRange(c, Mathf.RoundToInt(i + (side_x / 2) - centers_for_tangents[normal_index].x + d * Mathf.Cos(t)),
                                   Mathf.RoundToInt(j * side_y - side_y + centers_for_tangents[normal_index].y + d * Mathf.Sin(t)),
                                   Color.black, vert);
                    }
                    else // if ellipse nierby highest point of sin line
                    {
                        //getting distance
                        d = Vector2.Distance(centers_for_tangents[1], centers_for_tangents[normal_index == 2 ? 0 : 2]);

                        // left circle
                        if (is_bottom ? (t <= Mathf.PI - tan_sin || t >= tan1 + 2 * Mathf.PI) : (t <= tan2 || t >= 2 * Mathf.PI - tan_sin))
                            SetPixelsInRange(c, Mathf.RoundToInt(i - (side_x / 2) + centers_for_tangents[normal_index == 2 ? 0 : 2].x + d * Mathf.Cos(t)),
                                   Mathf.RoundToInt(j * side_y + side_y - centers_for_tangents[normal_index == 2 ? 0 : 2].y + d * Mathf.Sin(t)),
                                   Color.black, vert);

                        // right circle
                        if (is_bottom ? (t >= tan_sin && t <= Mathf.PI - tan1) : (t >= Mathf.PI - tan2 && t <= tan_sin + Mathf.PI))
                            SetPixelsInRange(c, Mathf.RoundToInt(i + (side_x / 2) - centers_for_tangents[normal_index == 2 ? 0 : 2].x + d * Mathf.Cos(t)),
                                   Mathf.RoundToInt(j * side_y + side_y - centers_for_tangents[normal_index == 2 ? 0 : 2].y + d * Mathf.Sin(t)),
                                   Color.black, vert);

                    }
                }
            }
        }

        void CalculateTangents(float rand, float sin, bool is_bottom, ref float tan1, ref float tan2, ref float tan_sin, ref Vector2[] centers_for_tangents)
        {
            float i = side_x / 2;

            tan_sin = GetSinNorm();

            float c_t1 = (rand > 0.5f ? sin + b : sin - b);
            c_t1 *= 1.1f;
            float c_t2 = (rand < 0.5f ? sin + b : sin - b);
            c_t2 *= 1.1f;



            for (float t = Mathf.PI; t >= Mathf.PI / 2; t -= Mathf.PI / 1800)
            {
                if (!is_bottom) _swap(ref c_t1, ref c_t2);

                if (centers_for_tangents[2] == Vector2Int.zero)
                {
                    _sendRay(new Vector2(i, side_y + c_t1), t, false);
                }
                if (tan1 == 0 && centers_for_tangents[2] != Vector2.zero)
                {

                    tan1 = Mathf.Tan(Mathf.Atan2((side_y + c_t1 - centers_for_tangents[2].y), (i - centers_for_tangents[2].x)));
                    break;
                }

            }
            for (float t = Mathf.PI; t <= 3 * Mathf.PI / 2; t += Mathf.PI / 1800)
            {
                if (!is_bottom) _swap(ref c_t1, ref c_t2);

                if (centers_for_tangents[0] == Vector2Int.zero)
                {
                    _sendRay(new Vector2(i, side_y + c_t2), t);
                }
                if (tan2 == 0 && centers_for_tangents[0] != Vector2.zero)
                {
                    tan2 = Mathf.Tan(Mathf.Atan2((side_y + c_t2 - centers_for_tangents[0].y), (i - centers_for_tangents[0].x)));
                    break;
                }

            }



        }

        float ellipse_angle(float rx, float ry, float ang)              // geometrical angle [rad] -> ellipse parametric angle [rad]
        {
            float x, y, t, aa, bb, ea;
            x = Mathf.Cos(ang);                                         // axis direction at angle ang
            y = Mathf.Sin(ang);
            aa = rx * rx; bb = ry * ry;                                 // intersection between ellipse and axis
            t = aa * bb / ((x * x * bb) + (y * y * aa));
            x *= t; y *= t;
            y *= rx / ry;                                               // convert to circle
            ea = Mathf.Atan2(y, x);                                     // compute elliptic angle
            if (ea < 0.0) ea += Mathf.PI * 2;                           // normalize to <0,pi2>
            return ea;
        }





        float GetSinNorm()
        {
            float sin_normal_y1 = (float)centers_for_tangents[1].y - ((1 - (float)centers_for_tangents[1].x) * side_x) / (sin_coef * Mathf.PI * Mathf.Cos(v));
            float sin_normal_y2 = (float)centers_for_tangents[1].y - ((-1 - (float)centers_for_tangents[1].x) * side_x) / (sin_coef * Mathf.PI * Mathf.Cos(v));
            return Mathf.Atan2(sin_normal_y1 - sin_normal_y2, 2);
        }

        Vector2 GetIntersectionPoint(Vector2 center, Vector2 tc, float tanc, float tansin)
        {
            float c1 = -tanc * center.x + center.y;
            float c2 = -tansin * tc.x + tc.y;

            float den = tanc - tansin;
            float temp_x = (c2 - c1) / den;
            float temp_y = tanc * temp_x + c1;
            return new Vector2(temp_x, temp_y);
        }

        void _sendRay(Vector2 center, float angle, bool is_bottom = true)
        {
            float distance_to_elipse = a * b / Mathf.Sqrt(a * a * Mathf.Sin(angle) * Mathf.Sin(angle) + b * b * Mathf.Cos(angle) * Mathf.Cos(angle));
            int index_n = is_bottom ? 0 : 2;

            Vector2 ip = GetIntersectionPoint(center, centers_for_tangents[1], Mathf.Tan(angle), Mathf.Tan(tan_sin));

            if (Mathf.Abs(Vector2.Distance(ip, centers_for_tangents[1]) -
                        (Vector2.Distance(ip, center) - distance_to_elipse)) < 0.5f)
            {
                centers_for_tangents[index_n] = ip;
            }


        }

    }
    private void _swap(ref float a, ref float b)
    {
        var t = a;
        a = b;
        b = t;
    }

}
