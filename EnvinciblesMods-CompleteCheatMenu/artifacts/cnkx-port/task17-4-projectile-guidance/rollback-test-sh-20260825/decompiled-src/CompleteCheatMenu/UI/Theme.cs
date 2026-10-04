using UnityEngine;

namespace CompleteCheatMenu.UI;

internal static class Theme
{
	internal static readonly Color Bg = Hex(1316636u);

	internal static readonly Color Sidebar = Hex(921878u);

	internal static readonly Color Panel = Hex(1777447u);

	internal static readonly Color PanelHi = Hex(2304051u);

	internal static readonly Color Border = Hex(3028033u);

	internal static readonly Color Accent = Hex(5030568u);

	internal static readonly Color AccentDim = Hex(3045995u);

	internal static readonly Color Danger = Hex(14702415u);

	internal static readonly Color TextMain = Hex(15133424u);

	internal static readonly Color TextMuted = Hex(9213349u);

	internal const float SidebarWidth = 132f;

	internal const float RowHeight = 24f;

	internal const float Pad = 10f;

	internal static GUIStyle Window;

	internal static GUIStyle SidebarBox;

	internal static GUIStyle TabNormal;

	internal static GUIStyle TabActive;

	internal static GUIStyle Section;

	internal static GUIStyle H1;

	internal static GUIStyle H2;

	internal static GUIStyle Body;

	internal static GUIStyle Muted;

	internal static GUIStyle Btn;

	internal static GUIStyle BtnAccent;

	internal static GUIStyle BtnDanger;

	internal static GUIStyle Toggle;

	internal static GUIStyle Field;

	internal static GUIStyle SliderBar;

	internal static GUIStyle SliderThumb;

	internal static GUIStyle ToastOk;

	internal static GUIStyle ToastBad;

	internal static GUIStyle Divider;

	private static bool _built;

	private static Texture2D _transparent;

	internal static void EnsureBuilt()
	{
		if (!_built || Window == null)
		{
			_built = true;
			Texture2D background = Rounded(48, 8, Bg, Border);
			Texture2D tex = Rounded(48, 8, Sidebar, Border);
			Texture2D tex2 = Rounded(32, 6, Panel, Border);
			Texture2D background2 = Rounded(32, 6, PanelHi, Accent);
			Texture2D background3 = Rounded(32, 6, PanelHi, Border);
			Texture2D background4 = Rounded(32, 6, PanelHi, Border);
			Texture2D background5 = Rounded(32, 6, Lighten(PanelHi, 0.1f), Accent);
			Texture2D background6 = Rounded(32, 6, AccentDim, Accent);
			Texture2D background7 = Rounded(32, 6, Mix(Panel, Danger, 0.35f), Danger);
			Texture2D background8 = Rounded(32, 6, Hex(724241u), Border);
			Texture2D background9 = Rounded(16, 4, Hex(724241u), Border);
			Texture2D background10 = Rounded(16, 8, Accent, Accent);
			Window = new GUIStyle(GUI.skin.window)
			{
				normal = 
				{
					background = background,
					textColor = TextMain
				},
				onNormal = 
				{
					background = background,
					textColor = TextMain
				},
				border = new RectOffset(10, 10, 10, 10),
				padding = new RectOffset(0, 0, 26, 0),
				fontSize = 13,
				fontStyle = FontStyle.Bold,
				alignment = TextAnchor.UpperCenter
			};
			SidebarBox = Boxed(tex, 10);
			SidebarBox.padding = new RectOffset(8, 8, 8, 8);
			Section = Boxed(tex2, 8);
			Section.padding = new RectOffset(10, 10, 8, 10);
			TabNormal = new GUIStyle(GUI.skin.button)
			{
				normal = 
				{
					background = Transparent(),
					textColor = TextMuted
				},
				hover = 
				{
					background = background3,
					textColor = TextMain
				},
				active = 
				{
					background = background3,
					textColor = TextMain
				},
				border = new RectOffset(8, 8, 8, 8),
				padding = new RectOffset(10, 6, 6, 6),
				margin = new RectOffset(0, 0, 1, 1),
				alignment = TextAnchor.MiddleLeft,
				fontSize = 12
			};
			TabActive = new GUIStyle(TabNormal)
			{
				normal = 
				{
					background = background2,
					textColor = TextMain
				},
				hover = 
				{
					background = background2,
					textColor = TextMain
				},
				fontStyle = FontStyle.Bold
			};
			H1 = new GUIStyle(GUI.skin.label)
			{
				fontSize = 15,
				fontStyle = FontStyle.Bold,
				normal = 
				{
					textColor = TextMain
				}
			};
			H2 = new GUIStyle(GUI.skin.label)
			{
				fontSize = 12,
				fontStyle = FontStyle.Bold,
				normal = 
				{
					textColor = Accent
				}
			};
			Body = new GUIStyle(GUI.skin.label)
			{
				fontSize = 12,
				normal = 
				{
					textColor = TextMain
				}
			};
			Muted = new GUIStyle(GUI.skin.label)
			{
				fontSize = 11,
				normal = 
				{
					textColor = TextMuted
				},
				wordWrap = true
			};
			Btn = new GUIStyle(GUI.skin.button)
			{
				normal = 
				{
					background = background4,
					textColor = TextMain
				},
				hover = 
				{
					background = background5,
					textColor = TextMain
				},
				active = 
				{
					background = background6,
					textColor = TextMain
				},
				border = new RectOffset(8, 8, 8, 8),
				padding = new RectOffset(8, 8, 5, 5),
				margin = new RectOffset(2, 2, 2, 2),
				fontSize = 12
			};
			BtnAccent = new GUIStyle(Btn)
			{
				normal = 
				{
					background = background6,
					textColor = TextMain
				},
				fontStyle = FontStyle.Bold
			};
			BtnDanger = new GUIStyle(Btn)
			{
				normal = 
				{
					background = background7,
					textColor = TextMain
				},
				hover = 
				{
					background = background7,
					textColor = Color.white
				}
			};
			Toggle = new GUIStyle(GUI.skin.toggle)
			{
				fontSize = 12,
				normal = 
				{
					textColor = TextMain
				},
				onNormal = 
				{
					textColor = TextMain
				}
			};
			Field = new GUIStyle(GUI.skin.textField)
			{
				normal = 
				{
					background = background8,
					textColor = TextMain
				},
				focused = 
				{
					background = background8,
					textColor = TextMain
				},
				border = new RectOffset(8, 8, 8, 8),
				padding = new RectOffset(7, 7, 4, 4),
				fontSize = 12
			};
			SliderBar = new GUIStyle(GUI.skin.horizontalSlider)
			{
				normal = 
				{
					background = background9
				},
				border = new RectOffset(4, 4, 4, 4),
				fixedHeight = 10f,
				margin = new RectOffset(2, 2, 8, 8)
			};
			SliderThumb = new GUIStyle(GUI.skin.horizontalSliderThumb)
			{
				normal = 
				{
					background = background10
				},
				active = 
				{
					background = background10
				},
				border = new RectOffset(6, 6, 6, 6),
				fixedWidth = 14f,
				fixedHeight = 14f
			};
			ToastOk = Boxed(Rounded(32, 6, Mix(Panel, Accent, 0.22f), Accent), 8);
			ToastOk.padding = new RectOffset(10, 10, 6, 6);
			ToastOk.normal.textColor = TextMain;
			ToastOk.fontSize = 12;
			ToastBad = new GUIStyle(ToastOk)
			{
				normal = 
				{
					background = Rounded(32, 6, Mix(Panel, Danger, 0.25f), Danger),
					textColor = TextMain
				}
			};
			Divider = new GUIStyle
			{
				normal = 
				{
					background = Solid(Border)
				},
				fixedHeight = 1f,
				margin = new RectOffset(0, 0, 6, 6)
			};
		}
	}

	private static GUIStyle Boxed(Texture2D tex, int border)
	{
		return new GUIStyle(GUI.skin.box)
		{
			normal = 
			{
				background = tex,
				textColor = TextMain
			},
			border = new RectOffset(border, border, border, border)
		};
	}

	private static Texture2D Rounded(int size, int radius, Color fill, Color border)
	{
		Texture2D texture2D = new Texture2D(size, size, TextureFormat.ARGB32, mipChain: false)
		{
			filterMode = FilterMode.Bilinear,
			hideFlags = HideFlags.HideAndDontSave
		};
		for (int i = 0; i < size; i++)
		{
			for (int j = 0; j < size; j++)
			{
				float num = CornerDistance(j, i, size, radius);
				if (num > (float)radius + 0.5f)
				{
					texture2D.SetPixel(j, i, Color.clear);
					continue;
				}
				bool flag = num > (float)radius - 1.5f || j == 0 || i == 0 || j == size - 1 || i == size - 1;
				texture2D.SetPixel(j, i, flag ? border : fill);
			}
		}
		texture2D.Apply();
		return texture2D;
	}

	private static float CornerDistance(int x, int y, int size, int radius)
	{
		float num = ((x < radius) ? radius : ((x > size - 1 - radius) ? (size - 1 - radius) : x));
		float num2 = ((y < radius) ? radius : ((y > size - 1 - radius) ? (size - 1 - radius) : y));
		return Mathf.Sqrt(((float)x - num) * ((float)x - num) + ((float)y - num2) * ((float)y - num2));
	}

	private static Texture2D Solid(Color c)
	{
		Texture2D texture2D = new Texture2D(1, 1, TextureFormat.ARGB32, mipChain: false);
		texture2D.hideFlags = HideFlags.HideAndDontSave;
		texture2D.SetPixel(0, 0, c);
		texture2D.Apply();
		return texture2D;
	}

	private static Texture2D Transparent()
	{
		return _transparent ?? (_transparent = Solid(new Color(0f, 0f, 0f, 0f)));
	}

	private static Color Hex(uint rgb)
	{
		return new Color((float)((rgb >> 16) & 0xFF) / 255f, (float)((rgb >> 8) & 0xFF) / 255f, (float)(rgb & 0xFF) / 255f, 1f);
	}

	private static Color Lighten(Color c, float amount)
	{
		return Color.Lerp(c, Color.white, amount);
	}

	private static Color Mix(Color a, Color b, float t)
	{
		return Color.Lerp(a, b, t);
	}
}
