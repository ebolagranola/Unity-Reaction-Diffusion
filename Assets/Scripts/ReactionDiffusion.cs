using UnityEngine;

public class ReactionDiffusion : MonoBehaviour
{
    public ComputeShader shader;
    public Material mat;

    [Header("Diffusion Settings")]
    [Range(0.01f, 0.1f)]
    public float feed = 0.05f;
    [Range(0.045f, 0.07f)]
    public float kill = 0.0642f;
    [Range(0.0f, 1.0f)]
    public float clamp = 0.2f;
    [Header("Background Settings")]
    public Color backgroundColor;
    [Header("Foreground Settings")]
    public Color foregroundColor;

    int kernel;
    public int resolution = 256;
    int threadCount = 8;
    RenderTexture tex;
    ComputeBuffer bufferGrid;
    ComputeBuffer bufferNext;
    Vector2[] grid;
    Vector2[] next;

    bool mouseLeft = false;
    bool mouseRight = false;

    void Start()
    {
        initShader();
        initGrids();
    }

    void Update()
    {
        handleInput();
        runShader();
        swap();
    }

    void handleInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            mouseLeft = true;
        }
        if (Input.GetMouseButtonDown(1))
        {
            mouseRight = true;
        }

        if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1))
        {
            mouseLeft = false;
            mouseRight = false;
        }

        if (mouseLeft)
        {
            grid[GetIndex(Input.mousePosition.x / (Screen.width / resolution), Input.mousePosition.y / (Screen.height / resolution))].y = 0.5f;
        }
        if (mouseRight)
        {
            grid[GetIndex(Input.mousePosition.x / (Screen.width / resolution), Input.mousePosition.y / (Screen.height / resolution))].x = 0.0f;
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            Debug.Log("S key was pressed.");
        }
    }

    int GetIndex(float mouseLocX, float mouseLocY)
    {
        int xWrap = Mathf.FloorToInt(mouseLocX) % resolution;
        int yWrap = Mathf.FloorToInt(mouseLocY) % resolution;
        if (xWrap < 0) xWrap += resolution;
        if (yWrap < 0) yWrap += resolution;

        return (yWrap * resolution) + xWrap;
    }

    void initShader()
    {
        kernel = shader.FindKernel("Diffuse");
        tex = new RenderTexture(resolution, resolution, 24);
        tex.enableRandomWrite = true;
        tex.Create();
    }

    void runShader()
    {
        bufferGrid = new ComputeBuffer(grid.Length, 8);
        bufferGrid.SetData(grid);

        bufferNext = new ComputeBuffer(next.Length, 8);
        bufferNext.SetData(next);

        shader.SetInt("res", resolution);
        shader.SetFloat("feed", feed);
        shader.SetFloat("kill", kill);
        shader.SetFloat("clamp", clamp);
        shader.SetVector("backgroundColor", backgroundColor);
        shader.SetVector("foregroundColor", foregroundColor);
        shader.SetBuffer(kernel, "bufferGrid", bufferGrid);
        shader.SetBuffer(kernel, "bufferNext", bufferNext);
        shader.SetTexture(kernel, "Result", tex);

        shader.Dispatch(kernel, resolution / threadCount, resolution / threadCount, 1);
        mat.mainTexture = tex;
    }

    void initGrids()
    {
        grid = new Vector2[resolution * resolution];
        next = new Vector2[resolution * resolution];

        for (int i = 0; i < grid.Length; i++)
        {
            grid[i].x = 1.0f;
            grid[i].y = 0.0f;
        }
    }

    void swap()
    {
        bufferGrid.GetData(grid);
        bufferNext.GetData(next);
        Release();

        Vector2[] tmp = grid;
        grid = next;
        next = tmp;
    }

    public void Release()
    {
        bufferGrid.Release();
        bufferNext.Release();
    }
}
