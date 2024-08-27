using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public enum MaskType { Classic, Hex }
public class MaskGenerator : MonoBehaviour
{
    
    private int side_number = 10;
    Vector2Int image_rec = new Vector2Int(1920, 1080); 
    private Texture2D mask;
    private Vector2 number_of_puzzles;
    private Vector2 pixel_sides;

    public MaskGenerator(Vector2Int res, int num, MaskType mt) 
    {
        side_number = num;
        image_rec = res;
        GenerateMask(mt);
    }

    public Texture2D GetMask() => mask;
    
    
    private void GenerateMask(MaskType mt) 
    {
        mask = GenerateEmpty(image_rec);
        number_of_puzzles = new Vector2(mask.width / (mask.height / side_number), side_number);
        pixel_sides = new Vector2((mask.width / number_of_puzzles.x), (mask.height / number_of_puzzles.y));
        DrawPuzzleMask(mask, mt);   
    }


    private Texture2D GenerateEmpty(Vector2Int rec) 
    {
        var t = new Texture2D(rec.x, rec.y);
        t.wrapMode = TextureWrapMode.Clamp;
        for(int i = 0; i < rec.x; i ++)
            for (int j = 0; j < rec.y; j++)
                t.SetPixel(i,j, Color.white);
        return t;

    }

    void DrawBorder(Texture2D c, float side_x ,float side_y, int width, float numb_of_puz ,bool vert = false) 
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
        float tan1 = 0, tan2 = 0, tan_sin =0;
       
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
                    //SetPixelsInRange(Mathf.RoundToInt(i), Mathf.RoundToInt(j * side_y + con_shift), Color.black, vert);
                    float ang = ellipse_angle(a, b, t);
                    //if (ang > -tan1 || ang < tan1 + Mathf.PI)
                   

                    //print(tan1 * Mathf.Rad2Deg + " | " + tan2 * Mathf.Rad2Deg);

                    if (is_bottom)  //if under the sin line
                    {
                        if (t < -Mathf.Atan(tan1) || t > Mathf.Atan(tan1) + Mathf.PI)
                        {
                            
                            //print(t * Mathf.Rad2Deg); 
                            SetPixelsInRange(c, Mathf.RoundToInt(i + a * Mathf.Cos(ang)), Mathf.RoundToInt(j * side_y + con_shift + b * Mathf.Sin(ang)), Color.black, vert);
                            //SetPixelsInRange(Mathf.RoundToInt(i + a * Mathf.Cos(t)), Mathf.RoundToInt(j * side_y + con_shift + a * Mathf.Sin(t)), Color.green, vert);

                        }
                    }
                    else            //if over the sin line
                    {
                        if (t < Mathf.Atan(tan2) + Mathf.PI || t > 2 * Mathf.PI - Mathf.Atan(tan2))
                        {
                            SetPixelsInRange(c, Mathf.RoundToInt(i + a * Mathf.Cos(ang)), Mathf.RoundToInt(j * side_y + con_shift + b * Mathf.Sin(ang)), Color.black, vert);
                            //SetPixelsInRange(Mathf.RoundToInt(i + a * Mathf.Cos(t)), Mathf.RoundToInt(j * side_y + con_shift + a * Mathf.Sin(t)), Color.green, vert);
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
                            SetPixelsInRange(c,Mathf.RoundToInt(i - (side_x / 2) + centers_for_tangents[normal_index == 2 ? 0 : 2].x + d * Mathf.Cos(t)),
                                   Mathf.RoundToInt(j * side_y + side_y - centers_for_tangents[normal_index == 2 ? 0 : 2].y + d * Mathf.Sin(t)),
                                   Color.black, vert);

                        // right circle
                        if (is_bottom ? (t >= tan_sin && t <= Mathf.PI - tan1) : (t >= Mathf.PI - tan2 && t <= tan_sin + Mathf.PI))
                            SetPixelsInRange(c,Mathf.RoundToInt(i + (side_x / 2) - centers_for_tangents[normal_index == 2 ? 0 : 2].x + d * Mathf.Cos(t)),
                                   Mathf.RoundToInt(j * side_y + side_y - centers_for_tangents[normal_index == 2 ? 0 : 2].y + d * Mathf.Sin(t)),
                                   Color.black, vert);

                    }
                }
            }
        }

        //SetPixelsInRange(Mathf.RoundToInt(centers_for_tangents[1].x), Mathf.RoundToInt(centers_for_tangents[1].y), Color.red, vert);

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

                    tan1 = Mathf.Tan(Mathf.Atan2((side_y + c_t1 - centers_for_tangents[2].y) , (i - centers_for_tangents[2].x)));

                    //for (float m = 0; m < 75; m += 0.1f)
                    //{
                    //    SetPixelsInRange(Mathf.RoundToInt(centers_for_tangents[2].x + m), Mathf.RoundToInt(centers_for_tangents[2].y + m * tan1), Color.blue, vert);
                    //    SetPixelsInRange(Mathf.RoundToInt(centers_for_tangents[2].x - m), Mathf.RoundToInt(centers_for_tangents[2].y - m * tan1), Color.blue, vert);
                    //    SetPixelsInRange(Mathf.RoundToInt(side_x - centers_for_tangents[2].x + m), Mathf.RoundToInt(centers_for_tangents[2].y + m * -tan1), Color.blue, vert);
                    //    SetPixelsInRange(Mathf.RoundToInt(side_x - centers_for_tangents[2].x - m), Mathf.RoundToInt(centers_for_tangents[2].y - m * -tan1), Color.blue, vert);

                    //}

                    //SetPixelsInRange(Mathf.RoundToInt(i), Mathf.RoundToInt(pixel_sides.y + c_t1), Color.red, vert);

                    //tan1 = (centers_for_tangents[2].y - side_y - c_t1) / (centers_for_tangents[2].x - i);
                    //print(tan1 + "|" + Mathf.Atan2((side_y + c_t1 - centers_for_tangents[2].y), (i - centers_for_tangents[2].x)));
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

                    tan2 = Mathf.Tan(Mathf.Atan2((side_y + c_t2 - centers_for_tangents[0].y) , (i - centers_for_tangents[0].x)));

                    //for (float m = 0; m < 75; m += 0.1f)
                    //{
                    //    SetPixelsInRange(Mathf.RoundToInt(centers_for_tangents[0].x + m), Mathf.RoundToInt(centers_for_tangents[0].y + m * tan2), Color.red, vert);
                    //    SetPixelsInRange(Mathf.RoundToInt(centers_for_tangents[0].x - m), Mathf.RoundToInt(centers_for_tangents[0].y - m * tan2), Color.red, vert);
                    //    SetPixelsInRange(Mathf.RoundToInt(side_x - centers_for_tangents[0].x + m), Mathf.RoundToInt(centers_for_tangents[0].y + m * -tan2), Color.red, vert);
                    //    SetPixelsInRange(Mathf.RoundToInt(side_x - centers_for_tangents[0].x - m), Mathf.RoundToInt(centers_for_tangents[0].y - m * -tan2), Color.red, vert);
                    //}

                    //tan2 = (centers_for_tangents[0].y - side_y - c_t2) / (centers_for_tangents[0].x - i);
                    //print(tan2 + "|" + Mathf.Atan2((side_y + c_t2 - centers_for_tangents[0].y), (i - centers_for_tangents[0].x)));
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

        Vector2 GetIntersectionPoint(Vector2 center , Vector2 tc, float tanc, float tansin) 
        {
            float c1 = - tanc * center.x + center.y;
            float c2 = - tansin * tc.x + tc.y;

            float den = tanc - tansin;    
            float temp_x = (c2 - c1) / den;
            float temp_y = tanc * temp_x + c1;
            return new Vector2(temp_x, temp_y);
        }

        //remake 
        void _sendRay(Vector2 center, float angle, bool is_bottom = true)
        {
            float distance_to_elipse = a * b / Mathf.Sqrt(a * a * Mathf.Sin(angle) * Mathf.Sin(angle) + b * b * Mathf.Cos(angle) * Mathf.Cos(angle));
            int index_n = is_bottom ? 0 : 2;

            Vector2 ip = GetIntersectionPoint(center, centers_for_tangents[1], Mathf.Tan(angle), Mathf.Tan(tan_sin));
            //SetPixelsInRange(Mathf.RoundToInt(ip.x), Mathf.RoundToInt(ip.y), Color.magenta,vert);

            if (Mathf.Abs(Vector2.Distance(ip, centers_for_tangents[1]) -
                        (Vector2.Distance(ip, center) - distance_to_elipse)) < 0.5f) {
                //print(Vector2.Distance(ip, centers_for_tangents[1]) + " | " + Vector2.Distance(ip, center) + " | " + ip + " | " + distance_to_elipse);
                //print(ip);
                //SetPixelsInRange(Mathf.RoundToInt(ip.x), Mathf.RoundToInt(ip.y), Color.red, vert);
                centers_for_tangents[index_n] = ip;
            }

            //for (float i = center.x - 1.5f * a; i < center.x + 1.5f * a; i += 0.005f)
            //{
            //    float sin_normal_y = (float)centers_for_tangents[1].y - ((i - (float)centers_for_tangents[1].x) * side_x) / (sin_coef * Mathf.PI * Mathf.Cos(v));
            //    float elipse_normal_y = (i - center.x) * Mathf.Tan(angle) + center.y;

            //    if (Mathf.Abs(sin_normal_y - elipse_normal_y) < 0.1f)
            //    {
            //        if (Mathf.Abs(Vector2.Distance(new Vector2(i, elipse_normal_y), centers_for_tangents[1]) -
            //            (Vector2.Distance(new Vector2(i, elipse_normal_y), center) - distance_to_elipse)) < 0.5f)
            //        {
            //            centers_for_tangents[index_n] = new Vector2(i, elipse_normal_y);

            //            print("a"+Vector2.Distance(ip, centers_for_tangents[1]) + " | " + (Vector2.Distance(ip, center) - distance_to_elipse) + " | " + ip + " | " );
            //            print("b"+centers_for_tangents[index_n] + "   " + ip + " | "+ Vector2.Distance(new Vector2(i, elipse_normal_y), centers_for_tangents[1]) + " | " + (Vector2.Distance(new Vector2(i, elipse_normal_y), center) - distance_to_elipse));
            //            break;
            //        }

            //    }
            //}

        }

    }

    private void SetPixelsInRange(Texture2D c, int x, int y, Color col, bool v)
    {
        if (v) _swap_int(ref x, ref y);

        c.SetPixel(x, y, col);

        if (x - 1 >= 0) c.SetPixel(x - 1, y, col);
        if (x + 1 < c.width) c.SetPixel(x + 1, y, col);
        if (y - 1 >= 0) c.SetPixel(x, y - 1, col);
        if (y + 1 < c.height) c.SetPixel(x, y + 1, col);
    }

    
    private void _swap(ref float a, ref float b)
    {
        var t = a;
        a = b;
        b = t;
    }

    private void _swap_int(ref int a, ref int b)
    {
        var t = a;
        a = b;
        b = t;
    }

    private void DrawHex(Texture2D c, Vector2 center, float radius, Color col, bool fill = false)
    {
        Vector2 st = center + new Vector2(radius * Mathf.Cos(0), radius * Mathf.Sin(0));

        for (float r = radius; r >= 0; r -= 1.5f)
        {
            for (float t = Mathf.PI / 3; t < 2 * Mathf.PI; t += Mathf.PI / 3)
            {
                var temp = center + new Vector2(r * Mathf.Cos(t), r * Mathf.Sin(t));
                DrawLine(c, st, temp, Color.black);
                st = temp;
            }
            if (!fill)
            {
                break;
            }
        }
    }

        private void DrawLine(Texture2D c, Vector2 start, Vector2 end, Color color)
    {
        int x0 = Mathf.RoundToInt(start.x);
        int y0 = Mathf.RoundToInt(start.y);
        int x1 = Mathf.RoundToInt(end.x);
        int y1 = Mathf.RoundToInt(end.y);

        int dx = Mathf.Abs(x1 - x0);
        int dy = Mathf.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int err = dx - dy;
        int x = x0, y = y0;

        while (true)
        {
            SetPixelsInRange(c, x, y, color, false);

            if (x == x1 && y == y1)
                break;

            int e2 = 2 * err;
            if (e2 > -dy)
            {
                err -= dy;
                x += sx;
            }
            if (e2 < dx)
            {
                err += dx;
                y += sy;
            }
        }

    }

    private void DrawHexGrid(Texture2D c, float side_x, Vector2 numberofp)
    {
        float radius = side_x / Mathf.Sqrt(3);
        numberofp.x =  (c.width - radius / 2) / (3f * radius) ;
        float shift = (c.width - radius / 2 - (numberofp.x % 1 >= 0.5 ? (int)numberofp.x + 0.5f : (int)numberofp.x) * radius * 3f)/2;
        
        for (float i = - side_x / 2; i <= c.height + side_x / 2; i += side_x)
        {
            for (float j = shift - radius * 3; j <= c.width; j += radius * 3)
            {
                if (j < c.width - radius * 1.5f)
                {
                    DrawHex(c, new Vector2(j + radius, i), radius, Color.black);
                }
                else
                {
                    DrawHex(c, new Vector2(j + radius, i), radius, Color.black, true);
                }
                if (j >= shift - 1 && i >= side_x / 2 && i <= c.height - side_x && j <= c.width - radius * 3 + 1)
                {
                    DrawHex(c, new Vector2(j + 5 * radius / 2, i + side_x / 2), radius, Color.black);
                }
                else
                {
                    DrawHex(c, new Vector2(j + 5 * radius / 2, i + side_x / 2), radius, Color.black, true);
                }
            }
        }
    }


    void DrawPuzzleMask(Texture2D c, MaskType t) 
    {
        if (t == MaskType.Classic)
        {
            DrawBorder(c, pixel_sides.x, pixel_sides.y, c.width, number_of_puzzles.y);
            DrawBorder(c, pixel_sides.y, pixel_sides.x, c.height, number_of_puzzles.x, true);
        }
        if(t == MaskType.Hex) 
        { 
            DrawHexGrid(c, pixel_sides.y, number_of_puzzles);
        }

        c.Apply();       
    }

    
}
