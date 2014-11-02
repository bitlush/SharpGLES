using System;
using System.Windows.Forms;
using SharpGLES;

namespace HelloTriangle
{
	public partial class GLESForm : Form
	{
		private EGLDisplay _display;
		private int _program;
		private GLBuffer<float> _buffer;

		public GLESForm()
		{
			SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
		}

		protected override void OnPaintBackground(PaintEventArgs e)
		{
			
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			Draw();

			_display.SwapBuffers();

			Invalidate();
		}

		private void Draw()
		{
			GLES20.Viewport(0, 0, Width, Height);

			GLES20.ClearColor(0f, 0f, 0f, 0f);
			GLES20.Clear(GLES20.GL_COLOR_BUFFER_BIT);

			GLES20.UseProgram(_program);

			_buffer.Position(0);

			GLES20.VertexAttribPointer(0, 3, GLES20.GL_FLOAT, false, 0, _buffer); 
			GLES20.EnableVertexAttribArray(0);
			GLES20.DrawArrays(GLES20.GL_TRIANGLES, 0, 3); 
		}

		protected override void OnHandleDestroyed(EventArgs e)
		{
			_display.Dispose();

			base.OnHandleDestroyed(e);
		}

		protected override void OnHandleCreated(EventArgs e)
		{
			base.OnHandleCreated(e);

			_display = new EGLDisplay(Handle);

			_program = LoadProgram(@"
				attribute vec4 vPosition;
				void main()            
				{                          
					gl_Position = vPosition;
				}
			", @"
				precision mediump float; 
				void main()
				{
					gl_FragColor = vec4(1.0, 0.0, 0.0, 1.0);
				}
			");

			float[] vertices = {
				0.0f, 0.5f, 0.0f, 
				-0.5f, -0.5f, 0.0f, 
				0.5f, -0.5f, 0.0f
			};

			_buffer = new GLBuffer<float>(vertices.Length);

			_buffer.Put(vertices);
		}

		private static int LoadProgram(string vertexShader, string fragmentShader)
		{
			int id = GLES20.CreateProgram();

			if (id == 0)
			{
				throw new ApplicationException("Could not create new program.");
			}

			int vertexShaderId = LoadShader(GLES20.GL_VERTEX_SHADER, vertexShader);
			int fragmentShaderId = LoadShader(GLES20.GL_FRAGMENT_SHADER, fragmentShader);

			GLES20.AttachShader(id, vertexShaderId);
			GLES20.AttachShader(id, fragmentShaderId);
			GLES20.LinkProgram(id);

			GLES20.DeleteShader(vertexShaderId);
			GLES20.DeleteShader(fragmentShaderId);

			int status;

			GLES20.GetProgramiv(id, GLES20.GL_LINK_STATUS, out status);

			if (status == 0)
			{
				string log = GLES20.GetProgramInfoLog(id);

				GLES20.DeleteProgram(id);

				throw new ApplicationException("Error linking program: " + log);
			}

			GLES20.ValidateProgram(id);

			GLES20.GetProgramiv(id, GLES20.GL_VALIDATE_STATUS, out status);

			if (status == 0)
			{
				string log = GLES20.GetProgramInfoLog(id);

				throw new ApplicationException("Results of validating program: " + status + ". Log: " + log);
			}

			CheckError("Program load");

			return id;
		}

		private static int LoadShader(int type, String source)
		{
			int id = GLES20.CreateShader(type);

			if (id == 0)
			{
				throw new ApplicationException("Could not create shader.");
			}

			GLES20.ShaderSource(id, source);

			GLES20.CompileShader(id);

			int status;

			GLES20.GetShaderiv(id, GLES20.GL_COMPILE_STATUS, out status);

			if (status == 0)
			{
				String log = GLES20.GetShaderInfoLog(id);

				GLES20.DeleteShader(id);

				throw new ApplicationException("Error compiling shader: " + log);
			}

			return id;
		}

		private static void CheckError(string operation)
		{
			int error;
			int lastError = GLES20.GL_NO_ERROR;

			while ((error = GLES20.GetError()) != GLES20.GL_NO_ERROR)
			{
				lastError = error;
			}

			if (lastError != GLES20.GL_NO_ERROR)
			{
				throw new ApplicationException(operation + " produced error code " + lastError + ".");
			}
		}
	}
}

