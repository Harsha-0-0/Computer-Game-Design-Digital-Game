using UnityEngine;

public class DashedBorder : MonoBehaviour
{
    public float width = 6f;
    public float height = 2f;
    public float dashLength = 0.2f;
    public float gapLength = 0.12f;
    public Color borderColor = new Color(0.55f, 0.37f, 0.24f, 1f);
    public float lineWidth = 0.04f;

    void Start()
    {
        DrawDashedBorder();
    }

    void DrawDashedBorder()
    {
        float halfW = width / 2f;
        float halfH = height / 2f;
        float cornerRadius = 0.3f;

        // Draw 4 sides as dashed lines
        DrawDashedLine(
            new Vector2(-halfW + cornerRadius, halfH),
            new Vector2(halfW - cornerRadius, halfH));   // Top
        DrawDashedLine(
            new Vector2(-halfW + cornerRadius, -halfH),
            new Vector2(halfW - cornerRadius, -halfH));  // Bottom
        DrawDashedLine(
            new Vector2(-halfW, -halfH + cornerRadius),
            new Vector2(-halfW, halfH - cornerRadius));  // Left
        DrawDashedLine(
            new Vector2(halfW, -halfH + cornerRadius),
            new Vector2(halfW, halfH - cornerRadius));   // Right
    }

    void DrawDashedLine(Vector2 start, Vector2 end)
    {
        Vector2 dir = (end - start).normalized;
        float totalLength = Vector2.Distance(start, end);
        float drawn = 0f;
        int index = 0;

        while (drawn < totalLength)
        {
            float segLen = Mathf.Min(dashLength, totalLength - drawn);
            Vector2 a = start + dir * drawn;
            Vector2 b = start + dir * (drawn + segLen);

            GameObject dash = new GameObject("Dash_" + index++);
            dash.transform.SetParent(transform);
            dash.transform.localPosition = new Vector3(
                (a.x + b.x) / 2f, (a.y + b.y) / 2f, -0.2f);
            dash.transform.localRotation = Quaternion.FromToRotation(
                Vector3.right,
                new Vector3(dir.x, dir.y, 0));

            SpriteRenderer sr = dash.AddComponent<SpriteRenderer>();
            sr.sprite = GetWhiteSprite();
            sr.color = borderColor;
            sr.sortingOrder = 6;
            dash.transform.localScale = new Vector3(segLen, lineWidth, 1f);

            drawn += dashLength + gapLength;
        }
    }

    Sprite GetWhiteSprite()
    {
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        return Sprite.Create(tex,
            new Rect(0, 0, 1, 1),
            new Vector2(0.5f, 0.5f), 1f);
    }
}