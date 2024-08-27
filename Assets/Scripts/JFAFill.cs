using UnityEngine;

public class JFAFill : MonoBehaviour
{
    public Texture2D inputTexture; // Input texture with the closed area
    public Color fillColor = Color.red; // Color to fill the area

    private Texture2D outputTexture;

    void Start()
    {
        // Create a new texture with the same dimensions as the input texture
        outputTexture = new Texture2D(inputTexture.width, inputTexture.height, TextureFormat.RGBA32, false);

        // Initialize the output texture with the input texture's pixels
        Color[] pixels = inputTexture.GetPixels();
        outputTexture.SetPixels(pixels);
        outputTexture.Apply();

        // Perform Jump Flooding Algorithm to fill the area
        JumpFloodingFill(outputTexture, fillColor);

        // Apply the filled texture to a material (optional)
        GetComponent<Renderer>().material.mainTexture = outputTexture;
    }

    void JumpFloodingFill(Texture2D texture, Color fillColor)
    {
        int width = texture.width;
        int height = texture.height;

        // Create a buffer to store the filled pixels
        Color[] filledPixels = new Color[width * height];

        // Copy the initial texture data into the buffer
        Color[] pixels = texture.GetPixels();
        System.Array.Copy(pixels, filledPixels, pixels.Length);

        int maxJumpDistance = Mathf.Max(width, height);
        int jumpDistance = maxJumpDistance / 2;

        while (jumpDistance > 0)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color currentColor = filledPixels[y * width + x];
                    if (currentColor.a == 0) // Check if the pixel is empty (transparent)
                    {
                        // Check neighboring pixels within the jump distance
                        for (int offsetY = -jumpDistance; offsetY <= jumpDistance; offsetY += jumpDistance)
                        {
                            for (int offsetX = -jumpDistance; offsetX <= jumpDistance; offsetX += jumpDistance)
                            {
                                int neighborX = x + offsetX;
                                int neighborY = y + offsetY;

                                if (neighborX >= 0 && neighborX < width && neighborY >= 0 && neighborY < height)
                                {
                                    Color neighborColor = filledPixels[neighborY * width + neighborX];
                                    if (neighborColor.a > 0) // If the neighbor pixel is filled
                                    {
                                        filledPixels[y * width + x] = neighborColor;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            jumpDistance /= 2;
        }

        // Set the filled pixels back to the texture
        texture.SetPixels(filledPixels);
        texture.Apply();
    }


}