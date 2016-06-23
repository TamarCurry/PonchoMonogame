using System;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Poncho;
using Poncho.Display;
using Poncho.Events;
using Poncho.Text;

namespace PonchoMonogame
{
	internal class MonogameView
	{
		private enum MouseButton
		{
			LEFT,
			RIGHT,
			MIDDLE
		}

		private struct MouseButtonState
		{
			public DisplayObject downTarget;
			public DisplayObject upTarget;

			public MouseButton button {get; private set; }
			public string downEventType { get; private set; }
			public string upEventType { get; private set; }
			public string clickEventType { get; private set; }

			public MouseButtonState(MouseButton button, string downEventType, string upEventType, string clickEventType)
			{
				this.button = button;
				this.downEventType = downEventType;
				this.upEventType = upEventType;
				this.clickEventType = clickEventType;
				downTarget = null;
				upTarget = null;
			}
		}

		private ushort _renderW;
		private ushort _renderH;
		private Vector2 _pivot;
		private Vector2 _mousePos;
		private Vector2[] _verts;
		private Rectangle _sourceRect;
		private MouseState _mouseState;
		private MouseState _prevMouseState;
		private SpriteBatch _spriteBatch;
		private MonogameFonts _fonts;
		private DisplayObject _mouseTarget;
		private DisplayObject _prevMouseTarget;
		private MonogameImages _images;
		private MouseButtonState[] _mouseButtonStates;
		
		// --------------------------------------------------------------
		public MonogameView(SpriteBatch spriteBatch, MonogameImages images, MonogameFonts fonts)
		{
			_spriteBatch = spriteBatch;
			_images = images;
			_fonts = fonts;
			_verts = new Vector2[4];

			_mouseButtonStates = new[]{
				new MouseButtonState(MouseButton.LEFT, MouseEvent.MOUSE_DOWN, MouseEvent.MOUSE_UP, MouseEvent.CLICK),
				new MouseButtonState(MouseButton.RIGHT, MouseEvent.RIGHT_DOWN, MouseEvent.RIGHT_UP, MouseEvent.RIGHT_CLICK),
				new MouseButtonState(MouseButton.MIDDLE, MouseEvent.MIDDLE_DOWN, MouseEvent.MIDDLE_UP, MouseEvent.MIDDLE_CLICK)
			};
		}
		
		// --------------------------------------------------------------
		public void Update()
		{
			_prevMouseTarget = _mouseTarget;
			_mouseTarget = null;
			_prevMouseState = _mouseState;
			_mouseState = Mouse.GetState();
			_mousePos.X = _mouseState.Position.X;
			_mousePos.Y = _mouseState.Position.Y;
			Matrix matrix = Matrix.Identity;
			Draw(App.stage, matrix, true);
			UpdateMouseTargetState();
		}
		
		// --------------------------------------------------------------
		private void Draw(DisplayObject displayObject, Matrix parentMatrix, bool mouseEnabled)
		{
			if(!displayObject.visible) return;
			
			Matrix matrix =  Matrix.CreateScale(displayObject.scaleX, displayObject.scaleY, 1) * Matrix.CreateRotationZ(MathHelper.ToRadians(displayObject.rotation)) * Matrix.CreateTranslation(displayObject.x, displayObject.y, 0) * parentMatrix;
			
			_renderW = 0;
			_renderH = 0;

			Sprite sprite = displayObject as Sprite;
			DisplayObjectContainer parent = displayObject as DisplayObjectContainer;

			if (displayObject is TextField)
			{
				RenderText(displayObject as TextField, matrix);
			}
			else if (sprite != null)
			{
				RenderSpriteImage(sprite, matrix);
			}

			if (_renderW != 0 && _renderH != 0 && mouseEnabled && displayObject.mouseEnabled)
			{
				// Use the mouse state to detect if this sprite is the current object the mouse is over or clicked on.
				
				// Grab the vertices for each corner of the sprite
				_verts[0].X = -_pivot.X;
				_verts[0].Y = -_pivot.Y;
				_verts[1].X = _renderW - _pivot.X;
				_verts[1].Y = -_pivot.Y;
				_verts[2].X = -_pivot.X;
				_verts[2].Y = _renderH - _pivot.Y;
				_verts[3].X = _renderW - _pivot.X;
				_verts[3].Y = _renderH - _pivot.Y;

				// transform the vertices
				for( int i = 0; i < 4; ++i )
				{
					ConvertPoint(ref _verts[i], matrix);
				}

				// sort them by y coordinates and then by x coordinates
				Array.Sort(_verts, 
					(j, k) => {
						if(j.Y < k.Y) return -1;
						if(j.Y > k.Y) return 1;
						if(j.X < k.X) return -1;
						if(j.X > k.X) return 1;
						return 0;
					}
				);
					
				// Check to see if the mouse is in the sprite bounds
				if(PointInTri(_mousePos, _verts[0], _verts[1], _verts[2]) || PointInTri(_mousePos, _verts[2], _verts[1], _verts[3]))
				{
					// if so, this is the active mouse target
					_mouseTarget = displayObject;
				}
			}
			
			// now, render all the children
			if(parent != null)
			{
				int n = parent.numChildren;
				for ( int i = 0; i < n; ++i )
				{
					Draw(parent.GetChildAt(i), matrix, mouseEnabled && parent.mouseChildren);
				}
			}
		}
		
		// --------------------------------------------------------------
		private bool RenderSpriteImage(Sprite sprite, Matrix matrix)
		{
			Texture2D texture = (sprite.image as MonogameImage)?.texture;

			if (texture == null || texture.IsDisposed) return false;

			// set the pivot
			_pivot.X = sprite.pivotX;
			_pivot.Y = sprite.pivotY;

			// grab the source rect from the texture
			_sourceRect.X = sprite.image.rect.x;
			_sourceRect.Y = sprite.image.rect.y;
			_sourceRect.Width = sprite.imageWidth;
			_sourceRect.Height = sprite.imageHeight;

			// setup the render with the appropriate matrix and draw it
			_spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, RasterizerState.CullNone, null, matrix);
			_spriteBatch.Draw(texture, Vector2.Zero, _sourceRect, Color.White, 0, _pivot, 1, SpriteEffects.None, 0);
			_spriteBatch.End();

			_renderW = sprite.imageWidth;
			_renderH = sprite.imageHeight;

			return true;
		}
		
		// --------------------------------------------------------------
		private bool RenderText(TextField textField, Matrix matrix)
		{
			if (!string.IsNullOrWhiteSpace(textField.text) && textField.format != null)
			{
				SpriteFont font = _fonts.GetFont(textField.format.font);
				if (font != null)
				{
					string text = textField.text;
					
					if (!textField.multiline)
					{
						Regex reg = new Regex("\n|\r", RegexOptions.IgnoreCase);
						text = reg.Replace(text, " ");
					}
					
					if (textField.clipOverflow)
					{
						_sourceRect.X = 0;
						_sourceRect.Y = 0;
						_renderW = textField.width;
						_renderH = textField.height;
						_sourceRect.Width = textField.width;
						_sourceRect.Height = textField.height;
						if (textField.wordWrap && _renderW > 0 && _renderH > 0)
						{
							WrapText(font, text, _renderW);
						}
					}
					else
					{
						Vector2 v = font.MeasureString(text);
						_renderW = (ushort) v.X;
						_renderH = (ushort) v.Y;
					}

					_pivot.X = textField.pivotX*_renderW;
					_pivot.Y = textField.pivotY*_renderH;

					_spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, RasterizerState.CullNone, null, matrix);
					_spriteBatch.DrawString(font, textField.text, Vector2.Zero, Color.Black, 0, _pivot, Vector2.One, SpriteEffects.None, 0);
					_spriteBatch.End();
					
					return true;
				}
			}

			return false;
		}
		
		// --------------------------------------------------------------
		public string WrapText(SpriteFont spriteFont, string text, ushort maxLineWidth)
		{
			string[] words = text.Split(' ');
			StringBuilder sb = new StringBuilder();
			float lineWidth = 0f;
			float spaceWidth = spriteFont.MeasureString(" ").X;

			foreach (string word in words)
			{
				Vector2 size = spriteFont.MeasureString(word);

				if (lineWidth + size.X < maxLineWidth)
				{
					sb.Append(word + " ");
					lineWidth += size.X + spaceWidth;
				}
				else
				{
					sb.Append("\n" + word + " ");
					lineWidth = size.X + spaceWidth;
				}
			}

			return sb.ToString();
		}
		
		// --------------------------------------------------------------
		/// <summary>
		/// Checks to see if the specified point lies within the three other points
		/// </summary>
		/// <param name="pt"></param>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <param name="c"></param>
		/// <returns></returns>
		private bool PointInTri( Vector2 pt, Vector2 a, Vector2 b, Vector2 c)
		{
			bool b1, b2, b3;

			b1 = GetSign(pt, a, b) < 0.0f;
			b2 = GetSign(pt, b, c) < 0.0f;
			b3 = GetSign(pt, c, a) < 0.0f;

			return ((b1 == b2) && (b2 == b3));
		}
		
		// --------------------------------------------------------------
		private float GetSign( Vector2 a, Vector2 b, Vector2 c )
		{
			return (a.X - c.X) * (b.Y - c.Y) - (b.X - c.X) * (a.Y - c.Y);
		}
		
		// --------------------------------------------------------------
		
		/// <summary>
		/// Adjusts the specified point using the specified matrix.
		/// </summary>
		/// <param name="pt"></param>
		/// <param name="m"></param>
		private void ConvertPoint( ref Vector2 pt, Matrix m )
		{
			float a		= m.M11;
			float b		= m.M12;
			float c		= m.M21;
			float d		= m.M22;
			float tx	= m.M41;
			float ty	= m.M42;
			
			float x		= (pt.X * a) + (pt.Y * c) + tx;
			float y		= (pt.X * b) + (pt.Y * d) + ty;

			pt.X		= x;
			pt.Y		= y;
		}
		
		// --------------------------------------------------------------
		private void UpdateMouseTargetState()
		{
			for ( int i = 0; i < _mouseButtonStates.Length; ++i )
			{
				ButtonState buttonState = _mouseState.LeftButton;
				ButtonState prevState = _prevMouseState.LeftButton;
				int delta = 0;

				if(_mouseButtonStates[i].button == MouseButton.RIGHT)
				{
					buttonState = _mouseState.RightButton;
					prevState = _prevMouseState.RightButton;
				}
				else if ( _mouseButtonStates[i].button == MouseButton.MIDDLE )
				{
					buttonState = _mouseState.MiddleButton;
					prevState = _prevMouseState.MiddleButton;
					delta = _mouseState.ScrollWheelValue - _prevMouseState.ScrollWheelValue;
				}

				if(_mouseTarget == _prevMouseTarget && _mouseTarget != null) // target hasn't changed
				{
					if(buttonState != prevState) // state changed
					{
						if(buttonState == ButtonState.Pressed) {
							_mouseButtonStates[i].downTarget = _mouseTarget;
							_mouseButtonStates[i].upTarget = null;
							_mouseTarget.DispatchEvent(new MouseEvent(_mouseButtonStates[i].downEventType));
							// dispatch down event for the button here
						}
						else
						{
							_mouseButtonStates[i].upTarget = _mouseTarget;
							_mouseTarget.DispatchEvent(new MouseEvent(_mouseButtonStates[i].upEventType));
							// dispatch up event for the button here
						}

						if(_mouseButtonStates[i].downTarget == _mouseButtonStates[i].upTarget)
						{
							// dispatch click event for the button here
							// clear targets
							_mouseButtonStates[i].downTarget = null;
							_mouseButtonStates[i].upTarget = null;
							_mouseTarget.DispatchEvent(new MouseEvent(_mouseButtonStates[i].clickEventType));
						}
					}

					if (delta != 0)
					{
						_mouseTarget.DispatchEvent(new MouseEvent(MouseEvent.MOUSE_WHEEL, null, delta));
					}
				}
				else // target changed, clear states
				{
					_mouseButtonStates[i].upTarget = null;
					_mouseButtonStates[i].downTarget = null;
				}
			}
			
			if(_prevMouseTarget != _mouseTarget)
			{
				if(_prevMouseTarget != null)
				{
					// mouse out event goes here
					_prevMouseTarget.DispatchEvent( new MouseEvent(MouseEvent.MOUSE_OUT, _mouseTarget));
				}

				if(_mouseTarget != null)
				{
					// mouse over event goes here
					_mouseTarget.DispatchEvent( new MouseEvent(MouseEvent.MOUSE_OVER, _prevMouseTarget));
				}
			}

			_prevMouseTarget = null;
		}

	}
}
