/*
This is free software distributed under the terms of the MIT license, reproduced below. It may be used for any purpose, including commercial purposes, at absolutely no cost. No paperwork, no royalties, no GNU-like "copyleft" restrictions. Just download and enjoy.

Copyright (c) 2014 Chimera Software, LLC

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using UnityEngine;
using System.Xml;
using System.Collections;
using TMPro;
using System.Collections.Generic;
using System;
using UnityEngine.EventSystems;
using UnityEngine.UI;



public class DetectTextClickTMPro : MonoBehaviour, IPointerClickHandler, IPointerUpHandler, IPointerDownHandler, ICanvasRaycastFilter {

	public PUTMPro entity;


	public bool TestForHit(Vector2 screenPoint, Camera eventCamera, Action<string, int> block) {

		RectTransform rectTransform = gameObject.transform as RectTransform;

		Vector2 touchPos;
		RectTransformUtility.ScreenPointToLocalPointInRectangle (rectTransform, screenPoint, eventCamera, out touchPos);


		string value = null;

		if (entity == null) {
			return false;
		}
		if(entity.text != null)
			value = entity.text.text;
		if(entity.textGUI != null)
			value = entity.textGUI.text;

		float minDistance = 999999;
		int minChar = -1;
		int linkID = 0;
		int clickedLinkID = -1;


		TMP_TextInfo textInfo = null;

		if(entity.text != null)
			textInfo = entity.text.textInfo;
		if(entity.textGUI != null)
			textInfo = entity.textGUI.textInfo;

		Vector3[] vertices = textInfo.meshInfo.vertices;
		UIVertex[] uiVertices = null; //textInfo.meshInfo.uiVertices;

		if (vertices != null || uiVertices != null) {
			for (int i = 0; i < textInfo.wordCount; i++) {

				TMP_WordInfo wordInfo = textInfo.wordInfo [i];
				int charCount = wordInfo.characterCount;

				TMP_CharacterInfo startCharInfo = textInfo.characterInfo [wordInfo.firstCharacterIndex];
				for (int j = 0; j < charCount; j++) {
				
					TMP_CharacterInfo charInfo = textInfo.characterInfo [wordInfo.firstCharacterIndex + j];
					int vertIndex = startCharInfo.vertexIndex;
					int index_X4 = j * 4;

					Vector3 a = (vertices != null ? vertices [vertIndex + 0 + index_X4] : uiVertices [vertIndex + 0 + index_X4].position);
					Vector3 b = (vertices != null ? vertices [vertIndex + 1 + index_X4] : uiVertices [vertIndex + 1 + index_X4].position);
					Vector3 c = (vertices != null ? vertices [vertIndex + 2 + index_X4] : uiVertices [vertIndex + 2 + index_X4].position);
					Vector3 d = (vertices != null ? vertices [vertIndex + 3 + index_X4] : uiVertices [vertIndex + 3 + index_X4].position);

					a = (a + b + c + d) / 4;

					if (charInfo.character == '\x0b') {
						linkID++;
					}
					
					float distance = Vector2.Distance (touchPos, a);
					if (distance < minDistance) {
						minDistance = distance;
						minChar = charInfo.index;
					}
				}
			}
		}

		if (minChar >= 0 && minDistance < 20 && minChar < value.Length) {
			// i is the index into the string which we clicked.  Determine a "link" by finding the previous '['
			// and the ending ']'

			// run backwards from here to find the link idex...
			clickedLinkID = -1;
			for (int k = minChar; k >= 0; k--) {
				if (value [k] == '\x0b') {
					clickedLinkID++;
				}
			}

			if(clickedLinkID == -1){
				return false;
			}

			int startIndex = -1;
			int endIndex = -1;
			for (int k = minChar; k >= 0; k--) {
				if (value [k] == '\x0b') {
					startIndex = k;
					break;
				}
				if (value [k] == '\x0c') {
					endIndex = -1;
					break;
				}
			}
			for (int k = minChar; k < value.Length; k++) {
				if (value [k] == '\x0c') {
					endIndex = k;
					break;
				}
				if (value [k] == '\x0b') {
					endIndex = -1;
					break;
				}
			}
				
			if (startIndex >= 0 && endIndex >= 0) {
				if (block != null) {
					string linkText = value.Substring (startIndex + 1, endIndex - startIndex - 1).Trim ();
					block (linkText, clickedLinkID);
				}
				return true;
			}
		}

		return false;
	}

	public bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera) {
		if (gameObject.activeSelf == false)
			return false;
		return TestForHit(screenPoint, eventCamera, null);
	}

	public void OnPointerClick(PointerEventData eventData) {
		if (gameObject.activeSelf == false)
			return;
		TestForHit (Input.mousePosition, eventData.pressEventCamera, (linkText, clickedLinkID) => {
			if(entity != null){
				entity.LinkClicked (linkText, clickedLinkID);
			}
		});
	}

	public void OnPointerDown(PointerEventData data) {

	}

	public void OnPointerUp(PointerEventData data) {
		
	}
}





public class PUTMPro : PUGameObject {

	static Dictionary<string,TextMeshProFont> fontAssets = new Dictionary<string, TextMeshProFont>();

	public static string DefaultFont = "Fonts/ArialRegular";

	public TextMeshProUGUI textGUI;
	public TextMeshPro text;

	public string onLinkClick;

	public Action<string, int> OnLinkClickAction;
	public Func<string, int, string> TranslateLinkAction;


	public string value = null;
	public bool sizeToFit = false;
	public bool enableWordWrapping = true;
	public int maxVisibleLines = 0;
	public int maxSize = 200;
	public int minSize = 0;
	public string font;
	public string fontStyle;
	public int fontSize = 32;
	public Color fontColor = Color.white;
	public TextAlignmentOptions alignment = TextAlignmentOptions.TopLeft;

	public PUTMPro() {

	}

	public void LinkClicked(string linkText, int linkID) {

		if (TranslateLinkAction != null) {
			linkText = TranslateLinkAction (linkText, linkID);
		}

		if (OnLinkClickAction != null) {
			OnLinkClickAction (linkText, linkID);
		}
		if (OnLinkClickAction == null && PUText.GlobalOnLinkClickAction != null) {
			PUText.GlobalOnLinkClickAction (linkText, linkID, this);
		}
		if (onLinkClick != null) {
			NotificationCenter.postNotification (Scope (), onLinkClick, NotificationCenter.Args ("link", linkText));
		}
	}

	static public TextMeshProFont GetFont(string path) {
		if (fontAssets.ContainsKey (path)) {
			return fontAssets [path];
		}
		TextMeshProFont font = Resources.Load<TextMeshProFont> (path);
		if (font != null) {
			fontAssets [path] = font;
		}
		return font;
	}

	public override void gaxb_final(XmlReader reader, object _parent, Hashtable args) {
		base.gaxb_final(reader, _parent, args);

		string attrib;

		if (font == null) {
			font = DefaultFont;
		}

		if(reader != null){
			attrib = reader.GetAttribute ("font");
			if (attrib != null) {
				font = attrib;
			}

			attrib = reader.GetAttribute ("fontSize");
			if (attrib != null) {
				fontSize = int.Parse (attrib);
			}

			attrib = reader.GetAttribute ("fontStyle");
			if (attrib != null) {
				fontStyle = attrib;
			}

			attrib = reader.GetAttribute ("sizeToFit");
			if (attrib != null) {
				sizeToFit = bool.Parse (attrib);
			}

			attrib = reader.GetAttribute ("maxSize");
			if (attrib != null) {
				maxSize = int.Parse (attrib);
			}

			attrib = reader.GetAttribute ("enableWordWrapping");
			if (attrib != null) {
				enableWordWrapping = bool.Parse (attrib);
			}

			attrib = reader.GetAttribute ("maxVisibleLines");
			if (attrib != null) {
				maxVisibleLines = int.Parse (attrib);
			}

			attrib = reader.GetAttribute ("minSize");
			if (attrib != null) {
				minSize = int.Parse (attrib);
			}

			attrib = reader.GetAttribute ("alignment");
			if (attrib != null) {
				alignment = (TextAlignmentOptions)Enum.Parse (typeof(TextAlignmentOptions), attrib);
			}

			attrib = reader.GetAttribute ("fontColor");
			if (attrib != null) {
				fontColor = fontColor.PUParse (attrib);
			}

			value = reader.GetAttribute ("value");
			value = PlanetUnityOverride.processString (_parent, value);
		}
	}

	public override void gaxb_complete()
	{
		base.gaxb_complete ();


		GenerateTextComponent ();

		if ((onLinkClick != null || OnLinkClickAction != null || PUText.GlobalOnLinkClickAction != null)) {
			gameObject.AddComponent<DetectTextClickTMPro> ();
			DetectTextClickTMPro script = gameObject.GetComponent<DetectTextClickTMPro> ();
			script.entity = this;

			if (gameObject.GetComponent<Graphic> () == null) {
				gameObject.AddComponent<InvisibleHitGraphic> ();
			}

			// TODO: This hack was necessary for getting clicking to work on TMPro...
			//LeanTween.delayedCall (0, () => {
			//	if(textGUI != null){
			//		textGUI.RegisterGraphicForCanvas();
			//	}
			//});
		}
	}

	// This is required for application-level subclasses
	public override void gaxb_init ()
	{
		base.gaxb_init ();
		gaxb_addToParent();
	}

	public virtual void GenerateTextComponent() {
		textGUI = gameObject.AddComponent<TextMeshProUGUI> ();
		textGUI.font = GetFont(font);
		textGUI.enableWordWrapping = enableWordWrapping;
		textGUI.richText = true;
		if(fontStyle != null){
			textGUI.fontStyle = (FontStyles)Enum.Parse (typeof(FontStyles), fontStyle);
		}
		textGUI.fontSize = fontSize;
		textGUI.OverflowMode = TextOverflowModes.Truncate;
		textGUI.extraPadding = true;

		if (maxVisibleLines > 0) {
			textGUI.maxVisibleLines = maxVisibleLines;
		}
		textGUI.enableAutoSizing = sizeToFit;
		textGUI.fontSizeMin = minSize;
		textGUI.fontSizeMax = maxSize;

		//public enum TextAlignmentOptions { TopLeft = 0, Top = 1, TopRight = 2, TopJustified = 3, Left = 4, Center = 5, Right = 6, Justified = 7, BottomLeft = 8, Bottom = 9, BottomRight = 10, BottomJustified = 11, BaselineLeft = 12, Baseline = 13, BaselineRight = 14, BaselineJustified = 15 };
		textGUI.alignment = alignment;

		textGUI.color = fontColor;
		textGUI.text = PlanetUnityStyle.ReplaceStyleTags(value);
	}




	public Vector2 CalculateTextSize (string text, float maxWidth) {
		
		GameObject obj = new GameObject ();
		RectTransform rt = obj.AddComponent<RectTransform> ();
		rt.SetParent (PlanetUnityGameObject.MainCanvas ().rectTransform, false);
		rt.sizeDelta = new Vector2 (maxWidth, 1);
		TextMeshProUGUI t = obj.AddComponent<TextMeshProUGUI> ();
		t.font = textGUI.font;
		t.enableKerning = textGUI.enableKerning;
		t.extraPadding = textGUI.extraPadding;
		t.enableWordWrapping = textGUI.enableWordWrapping;
		t.OverflowMode = TextOverflowModes.Overflow;
		t.fontSize = textGUI.fontSize;
		t.richText = textGUI.richText;
		t.fontStyle = textGUI.fontStyle;
		t.alignment = textGUI.alignment;
		t.outlineWidth = textGUI.outlineWidth;
		t.characterSpacing = textGUI.characterSpacing;
		t.lineSpacing = textGUI.lineSpacing;
		t.paragraphSpacing = textGUI.paragraphSpacing;
		t.text = text;

		t.ForceMeshUpdate ();
		Vector2 size = new Vector2(((t.preferredWidth+textGUI.fontSize) < maxWidth ? (t.preferredWidth+textGUI.fontSize) : maxWidth), t.preferredHeight);
		GameObject.Destroy (obj);
		
		return size;
		
	}
	
	public Vector2 CalculateTextSize (string text, float maxWidth, string font, float fontSize, bool enableWordWrapping) {
		
		GameObject obj = new GameObject ();
		RectTransform rt = obj.AddComponent<RectTransform> ();
		rt.SetParent (PlanetUnityGameObject.MainCanvas ().rectTransform, false);
		rt.sizeDelta = new Vector2 (maxWidth, 1);
		TextMeshProUGUI t = obj.AddComponent<TextMeshProUGUI> ();
		t.font = GetFont(font);
		t.enableWordWrapping = enableWordWrapping;
		t.OverflowMode = TextOverflowModes.Overflow;
		t.text = text;
		t.fontSize = fontSize;
		t.richText = true;
		t.ForceMeshUpdate ();
		Vector2 size = new Vector2(t.preferredWidth, t.preferredHeight);
		GameObject.Destroy (obj);
		
		return size;
		
	}
}
