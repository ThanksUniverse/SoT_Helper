//using SoT_Helper.Models;
//using Newtonsoft.Json;
//using SoT_Helper.Services;
//using System.Reflection;
//using System.Text.Json;
//using System.Collections.Immutable;
//using System;
//using System.Collections.Generic;
//using System.Drawing;
//using System.Windows.Forms;
//using System;
//using System.Collections.Generic;
//using System.Windows.Forms;
////using SharpDX;
////using SharpDX.Direct2D1;
////using SharpDX.Direct3D11;
////using SharpDX.DXGI;
////using SharpDX.Mathematics.Interop;
////using AlphaMode = SharpDX.Direct2D1.AlphaMode;
////using Device = SharpDX.Direct2D1.Device;
////using Factory = SharpDX.Direct2D1.Factory;
////using Bitmap = SharpDX.Direct2D1.Bitmap;
////using D2DDevice = SharpDX.Direct2D1.Device;
////using D2DFactory = SharpDX.Direct2D1.Factory;
////using D2DBitmap = SharpDX.Direct2D1.Bitmap;
////using PixelFormat = SharpDX.Direct2D1.PixelFormat;
////using BitmapInterpolationMode = SharpDX.Direct2D1.BitmapInterpolationMode;
//using DXGIFormat = SharpDX.DXGI.Format;
//using DXGIUsage = SharpDX.DXGI.Usage;
//using DXGIFactory1 = SharpDX.DXGI.Factory1;
//using SharpDX.WIC;

//using System.Drawing.Imaging;
//using Pfim;
//using System.IO;
//using System.Runtime.InteropServices;

//using SharpDX;
//using SharpDX.Direct3D;
//using SharpDX.Direct3D11;
//using SharpDX.DXGI;
//using Buffer = SharpDX.Direct3D11.Buffer;
//using Device = SharpDX.Direct3D11.Device;
//using SharpDX.D3DCompiler;
//using System.Drawing.Drawing2D;
//using Matrix = SharpDX.Matrix;

//namespace SoT_Helper
//{
//    public class IslandInfo
//    {
//        //public Bitmap Image { get; set; }
//        public ShaderResourceView Texture { get; set; }
//        public string Rawname { get; set; }
//        public string Name { get; set; }
//        public Vector2 Position { get; set; }
//        public float Rotation { get; set; }
//        public float Scale { get; set; }
//    }

//    public struct Vertex
//    {
//        public Vector3 Position;
//        public Vector2 TexCoord;

//        public Vertex(Vector3 position, Vector2 texCoord)
//        {
//            Position = position;
//            TexCoord = texCoord;
//        }
//    }

//    public class MapForm : Form
//    {
//        static string islandsfile = "islands.json";
//        static string offsetsfile = "offsets.json";
//        static List<IslandInfo> Islands = new List<IslandInfo>();

//        //private SharpDX.Direct2D1.DeviceContext _d2dContext;
//        //private D2DFactory _d2dFactory;
//        private SwapChain _swapChain;
//        //private RenderTarget _renderTarget;
//        //private SolidColorBrush _brush;
//        private Vector2 _translation = new Vector2(0, 0);
//        private float _zoom = 0.5f;
//        private SharpDX.Point _prevMousePosition;
//        private bool _isDragging;
//        private Vector2 _mapSize = new Vector2(10000, 10000); // Set the size of the full map (5000x5000 pixels, for example);
//        private DeviceContext _d3dContext;
//        private SharpDX.Direct3D11.Device1 _d3dDevice;
//        private RenderTargetView _renderTargetView;
//        private Buffer _vertexBuffer;
//        private Buffer _indexBuffer;
//        private Buffer _worldMatrixBuffer;
//        private SamplerState _samplerState;
//        private Buffer _triangleVertexBuffer;

//        private Buffer _viewMatrixBuffer;
//        private Buffer _projectionMatrixBuffer;


//        public MapForm()
//        {
//            //InitializeComponent();
//            //imageBox1.MouseWheel += ImageBox1_MouseWheel;
//            string executingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

//            if (File.Exists(Path.Combine(executingDirectory, islandsfile)))
//            {
//                using (StreamReader reader = File.OpenText(Path.Combine(executingDirectory, islandsfile)))
//                {
//                    //JObject myObject = JsonConvert.DeserializeObject<JObject>(reader.ReadToEnd());
//                    string jsonString = reader.ReadToEnd();

//                    Islands = JsonConvert.DeserializeObject<List<IslandInfo>>(jsonString);
//                }
                
//            }

//            Text = "Interactive Map";
//            Width = 1024;
//            Height = 768;
//            DoubleBuffered = false;

//            StartPosition = FormStartPosition.CenterScreen;

//            //InitializeDirectX();
//            //LoadIslands();
//            InitializeDirect3D();
//            LoadIslands3D();
//            Application.Idle += Application_Idle;

//            MouseWheel += OnMouseWheel;
//            MouseMove += OnMouseMove;
//        }

//        //private void InitializeDirectX()
//        //{
//        //    // Create DXGI factory and Direct2D factory
//        //    using (var dxgiFactory = new SharpDX.DXGI.Factory1())
//        //    using (_d2dFactory = new D2DFactory())
//        //    {
//        //        // Create Direct3D device and context
//        //        var d3dDevice = new SharpDX.Direct3D11.Device(SharpDX.Direct3D.DriverType.Hardware, DeviceCreationFlags.BgraSupport);
//        //        var d3dContext = new SharpDX.Direct3D11.DeviceContext(d3dDevice);

//        //        // Create Direct2D device and context
//        //        using (var dxgiDevice = d3dDevice.QueryInterface<SharpDX.DXGI.Device>())
//        //        {
//        //            var d2dDevice = new D2DDevice(dxgiDevice, new CreationProperties { Options = DeviceContextOptions.None });
//        //            _d2dContext = new SharpDX.Direct2D1.DeviceContext(d2dDevice, DeviceContextOptions.None);
//        //        }

//        //        // Create DXGI swap chain and associate it with the form
//        //        var swapChainDesc = new SwapChainDescription
//        //        {
//        //            BufferCount = 2,
//        //            Usage = DXGIUsage.RenderTargetOutput,
//        //            OutputHandle = this.Handle,
//        //            IsWindowed = true,
//        //            ModeDescription = new ModeDescription
//        //            {
//        //                Width = ClientSize.Width,
//        //                Height = ClientSize.Height,
//        //                RefreshRate = new Rational(60, 1),
//        //                Format = DXGIFormat.B8G8R8A8_UNorm
//        //            },
//        //            SampleDescription = new SampleDescription(1, 0),
//        //            Flags = SwapChainFlags.AllowModeSwitch,
//        //            SwapEffect = SwapEffect.Sequential
//        //        };

//        //        _swapChain = new SwapChain(dxgiFactory, d3dDevice, swapChainDesc);

//        //        // Create Direct2D render target
//        //        using (var backBuffer = Texture2D.FromSwapChain<Texture2D>(_swapChain, 0))
//        //        {
//        //            _renderTarget = new RenderTarget(_d2dFactory, backBuffer.QueryInterface<Surface>(), new RenderTargetProperties(new PixelFormat(Format.Unknown, AlphaMode.Premultiplied)));
//        //            _d2dContext.Target = new Bitmap1(_d2dContext, backBuffer.QueryInterface<Surface>());
//        //        }
//        //    }
//        //}

//        private void LoadIslands3D()
//        {
//            string imageFolder = "h:\\Modding\\Sea of Thieves\\Game Assets\\Game\\Textures\\WorldIslandMaps\\";

//            //var IslandsFound = Islands.Where(i => i.Texture != null).ToList();

//            // Load your island images and create ShaderResourceView objects for each
//            foreach (var island in Islands)
//            {
//                //wsp_feature_crooks_hollow
//                var imagepath = Path.Combine(imageFolder, island.Rawname + "_questgen_WorldMap.tga");
//                if (!File.Exists(imagepath))
//                {
//                    imagepath = Path.Combine(imageFolder, island.Rawname + "_1_questgen_WorldMap.tga");
//                }
//                if (!File.Exists(imagepath))
//                    continue;
//                using (var fileStream = new FileStream(imagepath, FileMode.Open))
//                {
//                    var image = Pfimage.FromStream(fileStream);
//                    unsafe
//                    {
//                        fixed (byte* pData = image.Data)
//                        {
//                            IntPtr dataPtr = (IntPtr)pData;
//                            DataRectangle dataRectangle = new DataRectangle(dataPtr, image.Stride);
//                            var texture2DDescription = new Texture2DDescription
//                            {
//                                Width = image.Width,
//                                Height = image.Height,
//                                ArraySize = 1,
//                                BindFlags = BindFlags.ShaderResource,
//                                Usage = ResourceUsage.Immutable,
//                                CpuAccessFlags = CpuAccessFlags.None,
//                                Format = Format.R8G8B8A8_UNorm,
//                                MipLevels = 1,
//                                OptionFlags = ResourceOptionFlags.None,
//                                SampleDescription = new SampleDescription(1, 0)
//                            };

//                            Texture2D? texture2D = new Texture2D(_d3dDevice, texture2DDescription, dataRectangle);
//                            //var shaderResourceView = new ShaderResourceView(_d3dDevice, texture2D);
//                            island.Texture = new ShaderResourceView(_d3dDevice, texture2D);
//                        }
//                    }
//                }
//            }
//        }

//        private void InitializeDirect3D()
//        {
//            // Create the Direct3D device and device context
//            using (var defaultDevice = new SharpDX.Direct3D11.Device(DriverType.Hardware, DeviceCreationFlags.BgraSupport))
//            {
//                _d3dDevice = defaultDevice.QueryInterface<SharpDX.Direct3D11.Device1>();
//            }
//            _d3dContext = new DeviceContext(_d3dDevice);

//            // Create the swap chain description
//            var swapChainDesc = new SwapChainDescription()
//            {
//                ModeDescription = new ModeDescription(Width, Height, new Rational(60, 1), Format.R8G8B8A8_UNorm),
//                SampleDescription = new SampleDescription(1, 0),
//                Usage = Usage.RenderTargetOutput,
//                BufferCount = 1,
//                OutputHandle = Handle,
//                IsWindowed = true,
//                SwapEffect = SwapEffect.Discard,
//                Flags = SwapChainFlags.None
//            };

//            // Create the DXGI factory and swap chain
//            using (var dxgiFactory = new Factory1())
//            {
//                _swapChain = new SwapChain(dxgiFactory, _d3dDevice, swapChainDesc);
//            }

//            // Create and set the render target view
//            using (var backBuffer = _swapChain.GetBackBuffer<Texture2D>(0))
//            {
//                _renderTargetView = new RenderTargetView(_d3dDevice, backBuffer);
//            }
//            _d3dContext.OutputMerger.SetRenderTargets(_renderTargetView);

//            string executingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
//            string vertexShaderFile = @"Shaders\vertexShader.hlsl";

//            var vertexShaderFilePath = Path.Combine(executingDirectory, vertexShaderFile);

//            var vertexShaderByteCode = ShaderBytecode.CompileFromFile(vertexShaderFilePath, "main", "vs_4_0", ShaderFlags.Debug);

//            string pixelShaderFile = @"Shaders\pixelShader.hlsl";
//            var pixelShaderFilePath = Path.Combine(executingDirectory, pixelShaderFile);
//            var pixelShaderByteCode = ShaderBytecode.CompileFromFile(pixelShaderFilePath, "main", "ps_4_0", ShaderFlags.Debug);
//            var vertexShader = new VertexShader(_d3dDevice, vertexShaderByteCode);
//            var pixelShader = new PixelShader(_d3dDevice, pixelShaderByteCode);

//            // Create the vertex buffer
//            var vertices = new[]
//            {
//                new Vertex(new Vector3(-0.5f, 0.5f, 0.0f), new Vector2(0.0f, 0.0f)),
//                new Vertex(new Vector3(0.5f, 0.5f, 0.0f), new Vector2(1.0f, 0.0f)),
//                new Vertex(new Vector3(0.5f, -0.5f, 0.0f), new Vector2(1.0f, 1.0f)),
//                new Vertex(new Vector3(-0.5f, -0.5f, 0.0f), new Vector2(0.0f, 1.0f))
//            };

//            var vertexBufferDesc = new BufferDescription
//            {
//                BindFlags = BindFlags.VertexBuffer,
//                Usage = ResourceUsage.Default,
//                CpuAccessFlags = CpuAccessFlags.None,
//                OptionFlags = ResourceOptionFlags.None,
//                SizeInBytes = Utilities.SizeOf<Vertex>() * vertices.Length
//            };

//            _vertexBuffer = Buffer.Create(_d3dDevice, vertices, vertexBufferDesc);

//            // Create the index buffer
//            var indices = new uint[] { 0, 1, 2, 0, 2, 3 };

//            var indexBufferDesc = new BufferDescription
//            {
//                BindFlags = BindFlags.IndexBuffer,
//                Usage = ResourceUsage.Default,
//                CpuAccessFlags = CpuAccessFlags.None,
//                OptionFlags = ResourceOptionFlags.None,
//                SizeInBytes = sizeof(uint) * indices.Length
//            };

//            _indexBuffer = Buffer.Create(_d3dDevice, indices, indexBufferDesc);

//            // Create the world matrix constant buffer
//            var worldMatrixBufferDesc = new BufferDescription
//            {
//                BindFlags = BindFlags.ConstantBuffer,
//                Usage = ResourceUsage.Dynamic,
//                CpuAccessFlags = CpuAccessFlags.Write,
//                OptionFlags = ResourceOptionFlags.None,
//                SizeInBytes = Utilities.SizeOf<Matrix>()
//            };

//            _worldMatrixBuffer = new Buffer(_d3dDevice, worldMatrixBufferDesc);

//            // Create the sampler state
//            var samplerDesc = new SamplerStateDescription()
//            {
//                Filter = Filter.MinMagMipLinear,
//                AddressU = TextureAddressMode.Wrap,
//                AddressV = TextureAddressMode.Wrap,
//                AddressW = TextureAddressMode.Wrap,
//                MipLodBias = 0.0f,
//                MaximumAnisotropy = 1,
//                ComparisonFunction = Comparison.Never,
//                BorderColor = new Color4(0, 0, 0, 0),
//                MinimumLod = 0,
//                MaximumLod = float.MaxValue
//            };

//            _samplerState = new SamplerState(_d3dDevice, samplerDesc);


//            // Create the input layout for the vertex shader
//            var inputElements = new[]
//            {
//                new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0),
//                new InputElement("TEXCOORD", 0, Format.R32G32_Float, 12, 0)
//            };

//            var inputLayout = new InputLayout(_d3dDevice, vertexShaderByteCode, inputElements);

//            // Set the vertex and pixel shaders, and the input layout
//            _d3dContext.InputAssembler.InputLayout = inputLayout;
//            _d3dContext.VertexShader.Set(vertexShader);
//            _d3dContext.PixelShader.Set(pixelShader);

//            // Set up the view and projection matrices
//            float aspectRatio = (float)Width / Height;
//            Matrix view = Matrix.LookAtLH(new Vector3(0, 0, -1), new Vector3(0, 0, 0), Vector3.UnitY);
//            Matrix projection = Matrix.OrthoLH(aspectRatio * 2 * 5000, 2 * 5000, 0.1f, 100.0f);
//            Matrix viewProjection = Matrix.Multiply(view, projection);

//            // Set the view-projection matrix as a constant buffer for the vertex shader
//            Buffer viewProjectionBuffer = new Buffer(_d3dDevice, Utilities.SizeOf<Matrix>(), ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
//            _d3dContext.UpdateSubresource(ref viewProjection, viewProjectionBuffer);
//            _d3dContext.VertexShader.SetConstantBuffer(0, viewProjectionBuffer);

//            // Create the view matrix buffer
//            _viewMatrixBuffer = new Buffer(_d3dDevice, Utilities.SizeOf<Matrix>(), ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);

//            // Create the projection matrix buffer
//            _projectionMatrixBuffer = new Buffer(_d3dDevice, Utilities.SizeOf<Matrix>(), ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);

//            // Set the view matrix
//            Matrix viewMatrix = Matrix.LookAtLH(new Vector3(0, 0, -10000), Vector3.Zero, Vector3.UnitY);
//            _d3dContext.UpdateSubresource(ref viewMatrix, _viewMatrixBuffer);
//            _d3dContext.VertexShader.SetConstantBuffer(2, _viewMatrixBuffer);

//            // Set the projection matrix
//            Matrix projectionMatrix = Matrix.PerspectiveFovLH((float)Math.PI / 4.0f, (float)Width / Height, 1.0f, 20000.0f);
//            _d3dContext.UpdateSubresource(ref projectionMatrix, _projectionMatrixBuffer);
//            _d3dContext.VertexShader.SetConstantBuffer(3, _projectionMatrixBuffer);

//            // Create triangle vertex buffer
//            var triangleVertices = new[]
//            {
//                new Vertex(new Vector3(-2500, -2500, 0), new Vector2(0, 1)),
//                new Vertex(new Vector3(2500, -2500, 0), new Vector2(1, 1)),
//                new Vertex(new Vector3(0, 2500, 0), new Vector2(0.5f, 0)),
//            };

//            _triangleVertexBuffer = Buffer.Create(_d3dDevice, BindFlags.VertexBuffer, triangleVertices);


//        }
//        private void Render()
//        {
//            // Clear the render target view
//            _d3dContext.ClearRenderTargetView(_renderTargetView, new SharpDX.Color(0.4f, 0.6f, 0.9f));

//            // Set the viewport
//            _d3dContext.Rasterizer.SetViewport(new Viewport(0, 0, Width, Height));

//            // Set the input topology and vertex buffer for the triangle
//            _d3dContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
//            _d3dContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(_triangleVertexBuffer, Utilities.SizeOf<Vertex>(), 0));

//            // Draw the triangle (no index buffer needed)
//            _d3dContext.Draw(3, 0);

//            // Set the input topology, vertex buffer, and index buffer for the islands
//            _d3dContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
//            _d3dContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(_vertexBuffer, Utilities.SizeOf<Vertex>(), 0));
//            _d3dContext.InputAssembler.SetIndexBuffer(_indexBuffer, Format.R32_UInt, 0);

//            // Set the world matrix and the texture for the islands
//            foreach (var island in Islands)
//            {
//                Matrix worldMatrix = Matrix.Translation((Vector3)island.Position);
//                _d3dContext.UpdateSubresource(ref worldMatrix, _worldMatrixBuffer);
//                _d3dContext.VertexShader.SetConstantBuffer(1, _worldMatrixBuffer);
//                _d3dContext.PixelShader.SetShaderResource(0, island.Texture);
//                _d3dContext.PixelShader.SetSampler(0, _samplerState);

//                // Draw the island
//                _d3dContext.DrawIndexed(6, 0, 0);
//            }

//            // Present the back buffer to the screen
//            _swapChain.Present(1, PresentFlags.None);
//        }

//        //private void Render()
//        //{
//        //     Clear the render target view
//        //    _d3dContext.ClearRenderTargetView(_renderTargetView, new SharpDX.Color(0.4f, 0.6f, 0.9f));
//        //    _d3dContext.ClearRenderTargetView(_renderTargetView, SharpDX.Color.Aquamarine);

//        //     Set the viewport
//        //    _d3dContext.Rasterizer.SetViewport(new Viewport(0, 0, Width, Height));

//        //     Set the input topology, vertex buffer, and index buffer
//        //    _d3dContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
//        //    _d3dContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(_vertexBuffer, Utilities.SizeOf<Vertex>(), 0));
//        //    _d3dContext.InputAssembler.SetIndexBuffer(_indexBuffer, Format.R32_UInt, 0);

//        //     Set the world matrix and the texture
//        //    foreach (var island in Islands)
//        //    {
//        //        Matrix worldMatrix = Matrix.Translation((Vector3)island.Position);
//        //        _d3dContext.UpdateSubresource(ref worldMatrix, _worldMatrixBuffer);
//        //        _d3dContext.VertexShader.SetConstantBuffer(1, _worldMatrixBuffer);
//        //        _d3dContext.PixelShader.SetShaderResource(0, island.Texture);
//        //        _d3dContext.PixelShader.SetSampler(0, _samplerState);

//        //         Draw the island
//        //        _d3dContext.DrawIndexed(6, 0, 0);
//        //    }

//        //     Present the back buffer to the screen
//        //    _swapChain.Present(1, PresentFlags.None);
//        //}

//        private Vertex[] CreateTexturedQuad(Vector2 position, Vector2 size)
//        {
//            float left = position.X - size.X / 2.0f;
//            float right = position.X + size.X / 2.0f;
//            float top = position.Y - size.Y / 2.0f;
//            float bottom = position.Y + size.Y / 2.0f;

//            return new[]
//            {
//                new Vertex(new Vector3(left, top, 0), new Vector2(0, 0)),
//                new Vertex(new Vector3(right, top, 0), new Vector2(1, 0)),
//                new Vertex(new Vector3(left, bottom, 0), new Vector2(0, 1)),
//                new Vertex(new Vector3(right, top, 0), new Vector2(1, 0)),
//                new Vertex(new Vector3(right, bottom, 0), new Vector2(1, 1)),
//                new Vertex(new Vector3(left, bottom, 0), new Vector2(0, 1)),
//            };
//        }

//        //private void LoadIslands()
//        //{
//        //    string imageFolder = "h:\\Modding\\Sea of Thieves\\Game Assets\\Game\\Textures\\WorldIslandMaps\\";

//        //    foreach (var island in Islands)
//        //    {
//        //        //wsp_feature_crooks_hollow
//        //        var imagepath = Path.Combine(imageFolder, island.Rawname + "_questgen_WorldMap.tga");
//        //        if(!File.Exists(imagepath)) 
//        //        {
//        //            imagepath = Path.Combine(imageFolder, island.Rawname + "_1_questgen_WorldMap.tga");
//        //        }

//        //        if (File.Exists(imagepath))
//        //        {
//        //            using (var fileStream = File.OpenRead(imagepath))
//        //            {
//        //                IImage image = Pfim.Pfimage.FromStream(fileStream);
//        //                //IImage image = Targa.Create(fileStream, default);
//        //                //SharpDX.Direct2D1.PixelFormat format;

//        //                DataStream dataStream = new DataStream(image.DataLen, true, true);
//        //                dataStream.Write(image.Data, 0, image.DataLen);
//        //                dataStream.Position = 0;

//        //                var properties = new BitmapProperties
//        //                {
//        //                    PixelFormat = new SharpDX.Direct2D1.PixelFormat(Format.R8G8B8A8_UNorm, AlphaMode.Premultiplied)
//        //                };

//        //                var islandImage = new SharpDX.Direct2D1.Bitmap(_d2dContext, new Size2(image.Width, image.Height), dataStream, image.Width * 4, properties);
//        //                island.Image = islandImage;
//        //            }

//        //        }
//        //        //island.Image = new Image<Bgr, byte>(bytes);
//        //        island.Scale = 0.6f;
//        //        island.Rotation = 0f;
//        //        //island.Position *= 100f;
//        //    }
//        //    // Create a list of IslandInfo objects with the appropriate properties.
//        //    //List<IslandInfo> islands = new List<IslandInfo>
//        //    //{
//        //    //    new IslandInfo { Image = new Image<Bgr, byte>("island1.tga"), Position = new PointF(100, 100), Rotation = 30, Scale = 1.0f },
//        //    //    new IslandInfo { Image = new Image<Bgr, byte>("island2.tga"), Position = new PointF(500, 200), Rotation = 0, Scale = 0.8f },
//        //    //    new IslandInfo { Image = new Image<Bgr, byte>("island3.tga"), Position = new PointF(300, 400), Rotation = -45, Scale = 1.2f },
//        //    //    // Add more islands as needed.
//        //    //};

//        //    // Determine the full map size.
//        //    int fullWidth = 5000; // Change this value based on your full map size.
//        //    int fullHeight = 5000; // Change this value based on your full map size.
//        //}

//        private void Application_Idle(object sender, EventArgs e)
//        {
//            Render();
//        }

//        protected override void OnResize(EventArgs e)
//        {
//            base.OnResize(e);

//            //if (_swapChain != null)
//            //{
//            //    _d2dContext.Target?.Dispose();
//            //    _renderTarget?.Dispose();
//            //    _swapChain.ResizeBuffers(0, ClientSize.Width, ClientSize.Height, Format.Unknown, SwapChainFlags.None);

//            //    using (var backBuffer = Texture2D.FromSwapChain<Texture2D>(_swapChain, 0))
//            //    {
//            //        _renderTarget = new RenderTarget(_d2dFactory, backBuffer.QueryInterface<Surface>(), new RenderTargetProperties(new PixelFormat(Format.Unknown, AlphaMode.Premultiplied)));
//            //        _d2dContext.Target = new Bitmap1(_d2dContext, backBuffer.QueryInterface<Surface>());
//            //    }

//            //    Invalidate();
//            //}
//        }

//        protected void OnMouseWheel(object sender, MouseEventArgs e)
//        {
//            //base.OnMouseWheel(e);

//            const float scaleFactor = 1.1f;

//            if (e.Delta > 0)
//            {
//                _zoom *= scaleFactor;
//            }
//            else
//            {
//                _zoom /= scaleFactor;
//            }
//        }

//        //private void ImageBox1_MouseWheel(object sender, MouseEventArgs e)
//        //{
//        //    const float scaleFactor = 1.1f;
//        //    if (e.Delta > 0)
//        //    {
//        //        imageBox1.SetZoomScale(imageBox1.ZoomScale * scaleFactor, e.Location);
//        //    }
//        //    else if (e.Delta < 0)
//        //    {
//        //        imageBox1.SetZoomScale(imageBox1.ZoomScale / scaleFactor, e.Location);
//        //    }
//        //}

//        protected override void OnMouseDown(MouseEventArgs e)
//        {
//            base.OnMouseDown(e);

//            if (e.Button == MouseButtons.Left)
//            {
//                _isDragging = true;
//                _prevMousePosition = new SharpDX.Point(e.X, e.Y);
//            }
//        }

//        protected override void OnMouseUp(MouseEventArgs e)
//        {
//            base.OnMouseUp(e);

//            if (e.Button == MouseButtons.Left)
//            {
//                _isDragging = false;
//            }
//        }

//        protected void OnMouseMove(object sender, MouseEventArgs e)
//        {
//            //base.OnMouseMove(e);

//            if (_isDragging)
//            {
//                _translation.X += e.X - _prevMousePosition.X;
//                _translation.Y += e.Y - _prevMousePosition.Y;
//                _prevMousePosition = new SharpDX.Point(e.X, e.Y);
//            }
//        }

//        //private void Render()
//        //{
//        //    _d2dContext.BeginDraw();
//        //    _d2dContext.Clear(SharpDX.Color.CornflowerBlue);

//        //    // Apply the zoom and translation transformations
//        //    _d2dContext.Transform = Matrix3x2.Scaling(_zoom) * Matrix3x2.Translation(_translation);

//        //    // Draw the background (full map) with the specified size
//        //    using (var brush = new SolidColorBrush(_d2dContext, SharpDX.Color.LightGray))
//        //    {
//        //        _d2dContext.FillRectangle(new SharpDX.RectangleF(0, 0, _mapSize.X, _mapSize.Y), brush);
//        //    }

//        //    // Draw the islands
//        //    foreach (var island in Islands.Where(i => i.Image != null))
//        //    {
//        //        _d2dContext.Transform =
//        //            _d2dContext.Transform *
//        //            Matrix3x2.Translation(island.Position) *
//        //            Matrix3x2.Rotation(island.Rotation * ((float)Math.PI / 180), new Vector2(island.Image.Size.Width / 2.0f, island.Image.Size.Height / 2.0f)) *
//        //            Matrix3x2.Scaling(island.Scale, island.Scale, new Vector2(island.Image.Size.Width / 2.0f, island.Image.Size.Height / 2.0f));

//        //        _d2dContext.DrawBitmap(island.Image, 1.0f, BitmapInterpolationMode.Linear);

//        //        _d2dContext.Transform = Matrix3x2.Scaling(_zoom) * Matrix3x2.Translation(_translation);
//        //    }

//        //    _d2dContext.EndDraw();
//        //}

//        //private void Render()
//        //{
//        //    _d2dContext.BeginDraw();
//        //    _d2dContext.Clear(SharpDX.Color.CornflowerBlue);

//        //    // Apply the zoom and translation transformations
//        //    _d2dContext.Transform = Matrix3x2.Scaling(_zoom) * Matrix3x2.Translation(_translation + (_mapSize / 2.0f));

//        //    // Draw the background (full map) with the specified size
//        //    using (var brush = new SolidColorBrush(_d2dContext, SharpDX.Color.CornflowerBlue))
//        //    {
//        //        _d2dContext.FillRectangle(new SharpDX.RectangleF(-_mapSize.X / 2.0f, -_mapSize.Y / 2.0f, _mapSize.X, _mapSize.Y), brush);
//        //    }

//        //    // Draw the islands as circles
//        //    foreach (var island in Islands)
//        //    {
//        //        using (var brush = new SolidColorBrush(_d2dContext, SharpDX.Color.DarkGreen))
//        //        {
//        //            float circleRadius = Math.Min(island.Scale, island.Scale) * 10.0f; // Adjust the circle radius based on the island scale
//        //            Vector2 circleCenter = island.Position + new Vector2(circleRadius, circleRadius);
//        //            _d2dContext.FillEllipse(new Ellipse(circleCenter, circleRadius, circleRadius), brush);
//        //        }
//        //    }

//        //    _d2dContext.EndDraw();
//        //}

//        //private void Render()
//        //{
//        //    _d2dContext.BeginDraw();

//        //    // Clear the render target
//        //    _d2dContext.Clear(new RawColor4(0.4f, 0.6f, 0.9f, 1.0f));

//        //    // Apply the zoom and translation transformations
//        //    _d2dContext.Transform = Matrix3x2.Scaling(_zoom) * Matrix3x2.Translation(_translation + (_mapSize / 2.0f));

//        //    // Draw the background (full map) with the specified size
//        //    using (var brush = new SolidColorBrush(_d2dContext, SharpDX.Color.CornflowerBlue))
//        //    {
//        //        _d2dContext.FillRectangle(new SharpDX.RectangleF(-_mapSize.X / 2.0f, -_mapSize.Y / 2.0f, _mapSize.X, _mapSize.Y), brush);
//        //    }
//        //    var _islands = Islands.Where(i => i.Image != null).ToList();
//        //    var islandsFound = Islands.Where(i => i.Image != null).ToList();

//        //    //Draw the islands
//        //    foreach (var island in Islands)
//        //    {
//        //        using (var brush = new SolidColorBrush(_d2dContext, SharpDX.Color.DarkGreen))
//        //        {
//        //            float circleRadius = Math.Min(island.Scale, island.Scale) * 10.0f; // Adjust the circle radius based on the island scale
//        //            Vector2 circleCenter = island.Position + new Vector2(circleRadius, circleRadius);
//        //            _d2dContext.FillEllipse(new Ellipse(circleCenter, circleRadius, circleRadius), brush);
//        //        }
//        //    }

//        //    // Draw the islands
//        //    foreach (var island in _islands)
//        //    {
//        //        _d2dContext.Transform =
//        //            _d2dContext.Transform *
//        //            Matrix3x2.Translation(island.Position);
//        //        //Matrix3x2.Translation(island.Position - new Vector2(island.Image.Size.Width / 2.0f, island.Image.Size.Height / 2.0f));

//        //        _d2dContext.Transform = Matrix3x2.Scaling(_zoom) * Matrix3x2.Translation(_translation + (_mapSize / 2.0f));

//        //        _d2dContext.DrawBitmap(island.Image, 1.0f, BitmapInterpolationMode.Linear);

//        //    }

//        //    // Draw the islands
//        //    //foreach (var island in islandsFound)
//        //    //{
//        //    //    _d2dContext.Transform =
//        //    //        _d2dContext.Transform *
//        //    //        Matrix3x2.Translation(island.Position);

//        //    //    _d2dContext.DrawBitmap(island.Image, 1.0f, BitmapInterpolationMode.Linear);

//        //    //    //_d2dContext.Transform = Matrix3x2.Scaling(_zoom) * Matrix3x2.Translation(_translation + (_mapSize / 2.0f));
//        //    //    _d2dContext.Transform = Matrix3x2.Scaling(_zoom) * Matrix3x2.Translation(_translation) * Matrix3x2.Scaling(0.3f,0.3f);
//        //    //}

//        //    ////Draw the islands
//        //    //foreach (var island in islandsFound)
//        //    //{
//        //    //    // Draw each island at its position with its rotation and scale
//        //    //    _d2dContext.PushAxisAlignedClip(new RawRectangleF(0, 0, island.Image.Size.Width, island.Image.Size.Height), AntialiasMode.PerPrimitive);
//        //    //    _d2dContext.Transform =
//        //    //        Matrix3x2.Translation(_translation) *
//        //    //        Matrix3x2.Scaling(_zoom) *
//        //    //        Matrix3x2.Translation(island.Position)
//        //    //        * Matrix3x2.Rotation(island.Rotation * ((float)Math.PI / 180), new Vector2(island.Image.Size.Width / 2.0f, island.Image.Size.Height / 2.0f))
//        //    //         * Matrix3x2.Scaling(island.Scale, island.Scale, new Vector2(island.Image.Size.Width / 2.0f, island.Image.Size.Height / 2.0f))
//        //    //         ;
//        //    //    //Matrix3x2.Scaling(island.Scale, new Vector2(island.Image.Size.Width / 2.0f, island.Image.Size.Height / 2.0f));

//        //    //    //_d2dContext.Transform =
//        //    //    //    _d2dContext.Transform *
//        //    //    //    Matrix3x2.Translation(island.Position) *
//        //    //    //    Matrix3x2.Rotation(island.Rotation * ((float)Math.PI / 180), new Vector2(island.Image.Size.Width / 2.0f, island.Image.Size.Height / 2.0f)) *
//        //    //    //    Matrix3x2.Scaling(ref island.Scale, out new Vector2(island.Image.Size.Width / 2.0f, island.Image.Size.Height / 2.0f));

//        //    //    _d2dContext.DrawBitmap(island.Image, new RawRectangleF(0, 0, island.Image.Size.Width * island.Scale * _zoom, island.Image.Size.Height * island.Scale * _zoom), 1.0f, BitmapInterpolationMode.Linear);
//        //    //    _d2dContext.PopAxisAlignedClip();
//        //    //    _d2dContext.Transform = Matrix3x2.Scaling(_zoom) * Matrix3x2.Translation(_translation);
//        //    //}

//        //    _d2dContext.EndDraw();
//        //    _swapChain.Present(0, PresentFlags.None);
//        //}


//        private void LoadAndPlaceIslands()
//        {
//            //List<IslandInfo> islandInfos = new List<IslandInfo>();

//            string imageFolder = "h:\\Modding\\Sea of Thieves\\Game Assets\\Game\\Textures\\WorldIslandMaps\\";

//            foreach(var island in Islands)
//            {
//                //wsp_feature_crooks_hollow
//                var imagepath = Path.Combine(imageFolder, island.Rawname + "_questgen_WorldMap.tga");
//                var bytes = File.ReadAllBytes(imagepath);
//                //island.Image = new Image<Bgr, byte>(bytes);
//                island.Scale = 1;
//            }
//            // Create a list of IslandInfo objects with the appropriate properties.
//            //List<IslandInfo> islands = new List<IslandInfo>
//            //{
//            //    new IslandInfo { Image = new Image<Bgr, byte>("island1.tga"), Position = new PointF(100, 100), Rotation = 30, Scale = 1.0f },
//            //    new IslandInfo { Image = new Image<Bgr, byte>("island2.tga"), Position = new PointF(500, 200), Rotation = 0, Scale = 0.8f },
//            //    new IslandInfo { Image = new Image<Bgr, byte>("island3.tga"), Position = new PointF(300, 400), Rotation = -45, Scale = 1.2f },
//            //    // Add more islands as needed.
//            //};

//            // Determine the full map size.
//            int fullWidth = 5000; // Change this value based on your full map size.
//            int fullHeight = 5000; // Change this value based on your full map size.

//            //// Create an empty full map image with the specified size.
//            //Image<Bgr, byte> fullMap = new Image<Bgr, byte>(fullWidth, fullHeight, new Bgr(Color.White));

//            //// Place, rotate, and scale each island on the full map.
//            //foreach (IslandInfo island in Islands)
//            //{
//            //    // Scale the island image.
//            //    Image<Bgr, byte> scaledIsland = new Image<Bgr, byte>((int)(island.Image.Width * island.Scale), (int)(island.Image.Height * island.Scale));
//            //    CvInvoke.Resize(island.Image, scaledIsland, new Size(scaledIsland.Width, scaledIsland.Height), 0, 0, Emgu.CV.CvEnum.Inter.Cubic);

//            //    // Rotate the scaled island image.
//            //    Mat rotationMatrix = new Mat();
//            //    PointF center = new PointF(scaledIsland.Width / 2f, scaledIsland.Height / 2f);
//            //    CvInvoke.GetRotationMatrix2D(center, island.Rotation, 1.0, rotationMatrix);
//            //    Image<Bgr, byte> rotatedIsland = new Image<Bgr, byte>(scaledIsland.Size);
//            //    CvInvoke.WarpAffine(scaledIsland, rotatedIsland, rotationMatrix, rotatedIsland.Size, 
//            //        Emgu.CV.CvEnum.Inter.Cubic, Emgu.CV.CvEnum.Warp.Default, Emgu.CV.CvEnum.BorderType.Default, new MCvScalar(255, 255, 255));

//            //    // Place the rotated and scaled island image on the full map.
//            //    fullMap.ROI = new Rectangle((int)island.Position.X, (int)island.Position.Y, rotatedIsland.Width, rotatedIsland.Height);
//            //    rotatedIsland.CopyTo(fullMap);
//            //    fullMap.ROI = Rectangle.Empty; // Reset the ROI to the entire full map.
//        }

//            // Assign the full map to the ImageBox control.
//            //imageBox1.Image = fullMap;
//    }
//}

