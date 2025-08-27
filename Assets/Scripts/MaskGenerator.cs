using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public enum MaskType { Classic, Hex, Tshape, Scale, Sshape, SnowFlake }
public class MaskGenerator
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

        void _sendRay(Vector2 center, float angle, bool is_bottom = true)
        {
            float distance_to_elipse = a * b / Mathf.Sqrt(a * a * Mathf.Sin(angle) * Mathf.Sin(angle) + b * b * Mathf.Cos(angle) * Mathf.Cos(angle));
            int index_n = is_bottom ? 0 : 2;

            Vector2 ip = GetIntersectionPoint(center, centers_for_tangents[1], Mathf.Tan(angle), Mathf.Tan(tan_sin));

            if (Mathf.Abs(Vector2.Distance(ip, centers_for_tangents[1]) -
                        (Vector2.Distance(ip, center) - distance_to_elipse)) < 0.5f) {
                centers_for_tangents[index_n] = ip;
            }


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

    private void DrawTshapeGrid(Texture2D c, Vector2 numberofp) 
    {
        numberofp.x = Mathf.Round(c.width / (c.height / numberofp.y));

        for (int i = 1; i < numberofp.x; i++) 
        {
            Vector2 start = new Vector2(c.width / numberofp.x * i, 0) ;
            Vector2 end = new Vector2( c.width / numberofp.x * i, c.height ) ;

            DrawLine(c, start, end, Color.black);
        }

        for (int i = 1; i < numberofp.y; i++)
        {
            Vector2 start = new Vector2(0,c.height / numberofp.y * i) ;
            Vector2 end = new Vector2(c.width, c.height / numberofp.y * i) ;

            DrawLine(c, start, end, Color.black);
        }

        for (int i = 0; i < numberofp.y; i++)
        {
            
            Vector2 offsety = new Vector2(0, c.height / numberofp.y) / 2;

            for (int j = 0; j < numberofp.x; j++)
            {
                float puzzle_width = c.width / numberofp.x;
                float offsetx = puzzle_width / 4;
               
                Vector2 start = new Vector2(puzzle_width * j + offsetx, c.height / numberofp.y * i) + offsety;
                Vector2 end = new Vector2(puzzle_width * (j + 1) - offsetx, c.height / numberofp.y * i) + offsety;

                DrawLine(c, start, end, Color.black);
            }
        }

        for (int i = 0; i < numberofp.x; i++)
        {

            Vector2 offsetx =  new Vector2( c.width / numberofp.x, 0) / 2;

            for (int j = 0; j < numberofp.y; j++)
            {
                float puzzle_height = c.height / numberofp.y;
                float offsety = puzzle_height / 4;

                Vector2 start = new Vector2( c.width / numberofp.x * i, puzzle_height * j + offsety) + offsetx;
                Vector2 end = new Vector2( c.width / numberofp.x * i, puzzle_height * (j + 1) - offsety) + offsetx;

                DrawLine(c, start, end, Color.black);
            }
        }


        for (int i = 0; i < numberofp.y; i++)
        {

            Vector2 offsety = new Vector2(0, c.height / numberofp.y) / 4;

            for (int j = 0; j < numberofp.x; j++)
            {
                float puzzle_width = c.width / numberofp.x;
                float offsetx = puzzle_width / 4;

                Vector2 start = new Vector2(puzzle_width * j + offsetx, c.height / numberofp.y * i) + offsety;
                Vector2 end = new Vector2(puzzle_width * (j + 1) - offsetx * 2, c.height / numberofp.y * i) + offsety;

                DrawLine(c, start, end, Color.black);

                start = new Vector2(puzzle_width * j + offsetx * 3, c.height / numberofp.y * i) + offsety;
                end = new Vector2(puzzle_width * (j + 1), c.height / numberofp.y * i) + offsety;

                DrawLine(c, start, end, Color.black);

                start = new Vector2(puzzle_width * j, c.height / numberofp.y * i) + offsety * 3;
                end = new Vector2(puzzle_width * (j + 1) - offsetx * 3, c.height / numberofp.y * i) + offsety * 3;

                DrawLine(c, start, end, Color.black);

                start = new Vector2(puzzle_width * j + offsetx * 2, c.height / numberofp.y * i) + offsety * 3;
                end = new Vector2(puzzle_width * (j + 1) - offsetx, c.height / numberofp.y * i) + offsety * 3;

                DrawLine(c, start, end, Color.black);

            }
        }

        for (int i = 0; i < numberofp.x; i++)
        {

            Vector2 offsetx = new Vector2(c.width / numberofp.x, 0) / 4;

            for (int j = 0; j < numberofp.y; j++)
            {
                float puzzle_height = c.height / numberofp.y;
                float offsety = puzzle_height / 4;

                Vector2 start = new Vector2(c.width / numberofp.x * i, puzzle_height * j + offsety * 2) + offsetx;
                Vector2 end = new Vector2(c.width / numberofp.x * i, puzzle_height * (j + 1) - offsety) + offsetx;

                DrawLine(c, start, end, Color.black);

                start = new Vector2(c.width / numberofp.x * i, puzzle_height * j) + offsetx;
                end = new Vector2(c.width / numberofp.x * i, puzzle_height * (j + 1) - offsety * 3) + offsetx;

                DrawLine(c, start, end, Color.black);



                start = new Vector2(c.width / numberofp.x * i, puzzle_height * j + offsety * 3) + offsetx * 3;
                end = new Vector2(c.width / numberofp.x * i, puzzle_height * (j + 1)) + offsetx * 3;

                DrawLine(c, start, end, Color.black);

                start = new Vector2(c.width / numberofp.x * i, puzzle_height * j + offsety) + offsetx * 3;
                end = new Vector2(c.width / numberofp.x * i, puzzle_height * (j + 1) - offsety * 2) + offsetx * 3;

                DrawLine(c, start, end, Color.black);
            }
        }
    }


    private void DrawCircle(Texture2D c, Vector2 center, float radius, Color color, Vector2 circle_limits) 
    {
        for(float angle = circle_limits.x; angle < circle_limits.y; angle += Mathf.PI / 720) 
        {
            int x = Mathf.RoundToInt(Mathf.Sin(angle) * radius + center.x);
            int y = Mathf.RoundToInt(Mathf.Cos(angle) * radius + center.y);
            SetPixelsInRange(c, x, y, color, false);
        }
    }


    private void DrawScaleGrid(Texture2D c, Vector2 numberofp) 
    {
        float radius = c.height / numberofp.y / 2;
        numberofp.x = Mathf.FloorToInt(c.width / radius / 2);
        float offset = (c.width - numberofp.x * radius * 2) / 2;
        //print(numberofp);
        for (int i = 0; i < numberofp.y; i++) 
        {
            for (int j = 0; j < numberofp.x; j++)
            {
                Vector2 center = new Vector2(j * 2 * radius + radius + offset , i * 2 * radius + radius);
                Vector2 lim = new Vector2(-Mathf.PI/2, Mathf.PI/2);
                DrawCircle(c, center, radius, Color.black, lim);
            }
        }

        for (int i = 0; i <= numberofp.x; i++) 
        {
            for (int j = 0; j < numberofp.y; j++)
            {
                Vector2 center = new Vector2(i * 2 * radius + offset, j * radius * 2);
                Vector2 lim = new Vector2(-Mathf.PI / 2, Mathf.PI / 2);
                DrawCircle(c, center, radius, Color.black, lim);
            }
        }

        c.Apply();

        for (int i = 0; i <= numberofp.x; i++) 
        {
            Vector2 center = new Vector2(i * 2 * radius + offset, c.height - 1);
            //Vector2 lim = new Vector2(0, Mathf.PI * 2);
            //DrawCircle(c, center, 20, Color.red, lim);
            FillArea(c, (int)center.x, (int)center.y,Color.black);
        }
        

    }


    public static void FillArea(Texture2D image, int startX, int startY, Color fillcolor)
    {
        if (image == null) return;

        // Get the image dimensions
        int width = image.width;
        int height = image.height;

        // Get the starting pixel color
        Color startColor = image.GetPixel(startX, startY);

        // If the starting pixel is already black, return
        if (startColor == Color.black) return;

        if (startColor == Color.white && fillcolor == Color.white) return;

        // Create a stack to manage the pixels to process
        Stack<Vector2Int> pixels = new Stack<Vector2Int>();
        pixels.Push(new Vector2Int(startX, startY));

        while (pixels.Count > 0)
        {
            Vector2Int current = pixels.Pop();
            int x = current.x;
            int y = current.y;

            // Skip if out of bounds or already black
            if (x < 0 || x >= width || y < 0 || y >= height || image.GetPixel(x, y) == Color.black || image.GetPixel(x, y) == fillcolor)
                continue;

            // Set the current pixel to black
            image.SetPixel(x, y, fillcolor);

            // Add neighbors to the stack
            pixels.Push(new Vector2Int(x + 1, y)); // Right
            pixels.Push(new Vector2Int(x - 1, y)); // Left
            pixels.Push(new Vector2Int(x, y + 1)); // Up
            pixels.Push(new Vector2Int(x, y - 1)); // Down
        }

        // Apply the changes to the texture
        image.Apply();
    }

    private void DrawSshapeGrid(Texture2D c, Vector2 numberofp, float line_spawn_probability) 
    {
        numberofp.x = Mathf.Round(c.width / (c.height / numberofp.y));
        float line_len_y = (c.height / numberofp.y) * 2 / 6;
        float line_len_x = (c.width / numberofp.x) * 2 / 6;

        Vector2 start = new Vector2(0, line_len_y * 2);
        Vector2 end;

        List<Vector2> startpoints = new List<Vector2>();

        for (int i = 3; start.y <= c.height + c.width; i--) 
        {
            end = new Vector2(line_len_x * i, 0) + start;
            DrawLine(c, start, end, Color.black);
            startpoints.Add(end);

            start += new Vector2(0, line_len_y * 3);
            if (i == 1) 
            {
                i = 5;
                start -= new Vector2(0, line_len_y * 2);
            }
        }

        foreach (var v in startpoints)
        {
            end = v;
            float t;
            while (end.y > 0 || end.x < c.width)
            {
                start = end;
                end = start + new Vector2(0, -line_len_y * 2);
                DrawLine(c, start, end, Color.black);

                t = Random.Range(0f, 1f);
                if (t < line_spawn_probability) DrawLine(c, start, start + new Vector2(0, line_len_y), Color.black);


                start = end;
                end = start + new Vector2(line_len_x * 4, 0);
                DrawLine(c, start, end, Color.black);

                t = Random.Range(0f, 1f);
                if (t < line_spawn_probability) DrawLine(c, start, start - new Vector2( 0, line_len_y), Color.black);
            }
        }

        startpoints.Clear();
        start = new Vector2(line_len_x, 0);

        for (int i = 4; start.x <= c.height + c.width; i++)
        {
            end = new Vector2(0,line_len_y * i) + start;
            DrawLine(c, start, end, Color.black);
            startpoints.Add(end);

            start += new Vector2(line_len_x * 3, 0);
            if (i == 4)
            {
                i = 0;
                start -= new Vector2(line_len_x * 2, 0);
            }
        }

        start = new Vector2(-2*line_len_x, 0);

        for (int i = 3; start.x > -c.height; i--)
        {
            end = new Vector2(0, line_len_y * i) + start;
            DrawLine(c, start, end, Color.black);
            startpoints.Add(end);

            start -= new Vector2(line_len_x * 3, 0);
            if (i == 0)
            {
                i = 4;
                start += new Vector2(line_len_x * 2, 0);
            }
        }
        foreach (var v in startpoints)
        {
            end = v;
            float t;
            while (end.y < c.height || end.x < c.width)
            {
                start = end;
                end = start + new Vector2(line_len_x * 2, 0);
                DrawLine(c, start, end, Color.black);

                t = Random.Range(0f, 1f);
                if (t < line_spawn_probability) DrawLine(c, start, start - new Vector2(line_len_x , 0), Color.black);


                start = end;
                end = start + new Vector2(0,line_len_y * 4);
                DrawLine(c, start, end, Color.black);

                t = Random.Range(0f, 1f);
                if (t < line_spawn_probability) DrawLine(c, start, start + new Vector2(line_len_x, 0), Color.black);

            }
        }
    }



    List<Vector2> snowflake = new List<Vector2>();
    private void DrawSnowflake(Texture2D c, Vector2 center, float radius, float iter, Color color)
    {
        List<Vector2> snowflakepoints = new List<Vector2>();
        snowflakepoints.Add(center + new Vector2(0, radius));
        snowflakepoints.Add(center + new Vector2(radius * Mathf.Cos(-Mathf.PI / 6), radius * Mathf.Sin(-Mathf.PI / 6)));
        snowflakepoints.Add(center + new Vector2(radius * Mathf.Cos(Mathf.PI * 7 / 6), radius * Mathf.Sin(Mathf.PI * 7 / 6)));

        if (snowflake.Count == 0)
        {
            for (int o = 0; o < iter; o++)
            {
                if (Vector2.Distance(snowflakepoints[0], snowflakepoints[1]) < 10) break;

                List<Vector2> temp = new List<Vector2>();
                for (int i = 0; i < snowflakepoints.Count; i++)
                {
                    temp.Add(snowflakepoints[i]);
                    if (i == snowflakepoints.Count - 1)
                    {
                        foreach (var v in AddTriangleonLine(snowflakepoints[snowflakepoints.Count - 1], snowflakepoints[0]))
                            temp.Add(v);
                    }
                    else
                    {
                        foreach (var v in AddTriangleonLine(snowflakepoints[i], snowflakepoints[i + 1]))
                            temp.Add(v);
                    }
                }
                snowflakepoints = temp;
            }

            for (int i_vec = 0; i_vec < snowflake.Count; i_vec++)
            {
                snowflake.Add(snowflakepoints[i_vec] - center);
            }
        }
        else
        {

            for (int i_vec = 0; i_vec < snowflakepoints.Count; i_vec++)
            {
                snowflakepoints.Add(snowflake[i_vec] + center);
            }
        }

        for (int i = 0; i < snowflakepoints.Count; i++)
        {
            if (i == snowflakepoints.Count - 1)
            {
                DrawLine(c, snowflakepoints[snowflakepoints.Count - 1], snowflakepoints[0], color);
            }
            else
            {
                DrawLine(c, snowflakepoints[i], snowflakepoints[i + 1], color);
            }
        }

    }

    private List<Vector2> AddTriangleonLine(Vector2 start,Vector2 end) 
    {

        Vector2 point3 = (end - start) * 2 / 3 + start;
        Vector2 point1 = (end - start) / 3 + start;

        float t = Mathf.Atan2(point3.y - point1.y, point3.x - point1.x);

        Vector2 point2 = (point3 - point1) / 2 + point1;

        Vector2 temp = point3 - point2;

        point2 = new Vector2(-temp.y, temp.x) * Mathf.Sqrt(3) + point2;
        List<Vector2> newtriangle = new List<Vector2>() { point1, point2, point3 };

        return newtriangle;
    }


    private void DrawSnowFlakeGrid(Texture2D c,Vector2 numberofp, float iter) 
    {
        float radius = c.height / numberofp.y / 2;

        numberofp.x = Mathf.Floor((c.width - 2 * radius) / (radius * 2 * Mathf.Sqrt(3))) + 1;
        float offset_x = ((c.width - radius * 2) - radius * 2 * Mathf.Sqrt(3) * (numberofp.x - 1)) / 2;

        for (int j = -1; j < numberofp.x + 1; j++)
        {
            for (int i = 0; i < numberofp.y + 1; i++)
            {
                Vector2 center;
                if (i < numberofp.y)
                {
                    center = new Vector2(radius * 2 * Mathf.Sqrt(3) * j + radius + offset_x, radius * 2 * i + radius);
                    DrawSnowflake(c, center, radius, iter, Color.black);
                }
                if (j < numberofp.x)
                {
                    center = new Vector2((radius * 2 * Mathf.Sqrt(3) * j + radius) + Mathf.Sqrt(3) * radius + offset_x, radius * 2 * i);
                    DrawSnowflake(c, center, radius, iter, Color.black);
                }
            }
        }

        void CheckLine(int line, Color fill) 
        {
            int number_of_white_pixels = 0;
            for (int i = 0; i < c.height; i++)
            {
                if (c.GetPixel(line, i) != Color.black)
                {
                    number_of_white_pixels++;
                }
                else
                {
                    if (number_of_white_pixels < radius / 2)
                    {
                        FillArea(c, line, i - 1, fill);
                    }
                    number_of_white_pixels = 0;
                }
            }
        }

        CheckLine(1, Color.red);
        CheckLine(c.width-2, Color.red);

        CheckLine((int)(1 + offset_x), Color.white);
        CheckLine((int)(c.width - offset_x - 2), Color.white);

        SwapRedToBlack(c);

        void SwapRedToBlack(Texture2D texture)
        {
            Color[] pixels = texture.GetPixels();

            for (int i = 0; i < pixels.Length; i++)
            {
                if (pixels[i] == Color.red) // Exact match with Color.red
                {
                    pixels[i] = Color.black;
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();
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
        if (t == MaskType.Tshape)
        {
            DrawTshapeGrid(c, number_of_puzzles);
        }
        if (t == MaskType.Scale)
        {
            DrawScaleGrid(c, number_of_puzzles);
        }
        if (t == MaskType.Sshape)
        {
            DrawSshapeGrid(c, number_of_puzzles,0.05f);
        }
        if(t == MaskType.SnowFlake) 
        {
            DrawSnowFlakeGrid(c, number_of_puzzles,6); 
        }

        c.Apply();       
    }

    
}
