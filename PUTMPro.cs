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

	private void IsCloserToPoint(Vector3 mousePoint, Vector3 testPoint, TMP_LinkInfo testLink, ref float minDistance, ref TMP_LinkInfo minLinkInfo) {
		float testDistance = Vector3.SqrMagnitude (mousePoint - testPoint);
		if (testDistance < minDistance) {
			minDistance = testDistance;
			minLinkInfo = testLink;
		}
	}

	public bool TestForHit(Vector2 screenPoint, Camera eventCamera, Action<string, int> block) {

		TMP_TextInfo textInfo = null;
		TMP_Text text = null;

		if (entity.text != null) {
			text = entity.text;
			textInfo = entity.text.textInfo;
		}
		if (entity.textGUI != null) {
			text = entity.textGUI;
			textInfo = entity.textGUI.textInfo;
		}

		if (textInfo != null) {



			// find the closest link to our touch point
			float minDistance = 999999;
			TMP_LinkInfo minLinkInfo = new TMP_LinkInfo ();

			Transform rectTransform = text.transform;
			Vector3 position = screenPoint;

			// Convert position into Worldspace coordinates
			TMP_TextUtilities.ScreenPointToWorldPointInRectangle (rectTransform, position, eventCamera, out position);

			for (int i = 0; i < text.textInfo.linkCount; i++) {
				TMP_LinkInfo linkInfo = text.textInfo.linkInfo [i];

				bool isBeginRegion = false;

				Vector3 bl = Vector3.zero;
				Vector3 tl = Vector3.zero;
				Vector3 br = Vector3.zero;
				Vector3 tr = Vector3.zero;

				// Iterate through each character of the word
				for (int j = 0; j < linkInfo.linkTextLength; j++) {
					int characterIndex = linkInfo.linkTextfirstCharacterIndex + j;
					TMP_CharacterInfo currentCharInfo = text.textInfo.characterInfo [characterIndex];
					int currentLine = currentCharInfo.lineNumber;

					// Check if Link characters are on the current page
					if (text.overflowMode == TextOverflowModes.Page && currentCharInfo.pageNumber + 1 != text.pageToDisplay)
						continue;

					if (isBeginRegion == false) {
						isBeginRegion = true;

						bl = rectTransform.TransformPoint (new Vector3 (currentCharInfo.bottomLeft.x, currentCharInfo.descender, 0));
						tl = rectTransform.TransformPoint (new Vector3 (currentCharInfo.bottomLeft.x, currentCharInfo.ascender, 0));

						// If Word is one character
						if (linkInfo.linkTextLength == 1) {
							isBeginRegion = false;

							br = rectTransform.TransformPoint (new Vector3 (currentCharInfo.topRight.x, currentCharInfo.descender, 0));
							tr = rectTransform.TransformPoint (new Vector3 (currentCharInfo.topRight.x, currentCharInfo.ascender, 0));

							// Check for Intersection
							IsCloserToPoint (position, bl, linkInfo, ref minDistance, ref minLinkInfo);
							IsCloserToPoint (position, tl, linkInfo, ref minDistance, ref minLinkInfo);
							IsCloserToPoint (position, tr, linkInfo, ref minDistance, ref minLinkInfo);
							IsCloserToPoint (position, br, linkInfo, ref minDistance, ref minLinkInfo);
							IsCloserToPoint (position, (bl + br + tl + tr) * 0.25f, linkInfo, ref minDistance, ref minLinkInfo);
						}
					}

					// Last Character of Word
					if (isBeginRegion && j == linkInfo.linkTextLength - 1) {
						isBeginRegion = false;

						br = rectTransform.TransformPoint (new Vector3 (currentCharInfo.topRight.x, currentCharInfo.descender, 0));
						tr = rectTransform.TransformPoint (new Vector3 (currentCharInfo.topRight.x, currentCharInfo.ascender, 0));

						IsCloserToPoint (position, bl, linkInfo, ref minDistance, ref minLinkInfo);
						IsCloserToPoint (position, tl, linkInfo, ref minDistance, ref minLinkInfo);
						IsCloserToPoint (position, tr, linkInfo, ref minDistance, ref minLinkInfo);
						IsCloserToPoint (position, br, linkInfo, ref minDistance, ref minLinkInfo);
						IsCloserToPoint (position, (bl + br + tl + tr) * 0.25f, linkInfo, ref minDistance, ref minLinkInfo);
					}
					// If Word is split on more than one line.
					else if (isBeginRegion && currentLine != text.textInfo.characterInfo [characterIndex + 1].lineNumber) {
						isBeginRegion = false;

						br = rectTransform.TransformPoint (new Vector3 (currentCharInfo.topRight.x, currentCharInfo.descender, 0));
						tr = rectTransform.TransformPoint (new Vector3 (currentCharInfo.topRight.x, currentCharInfo.ascender, 0));

						IsCloserToPoint (position, bl, linkInfo, ref minDistance, ref minLinkInfo);
						IsCloserToPoint (position, tl, linkInfo, ref minDistance, ref minLinkInfo);
						IsCloserToPoint (position, tr, linkInfo, ref minDistance, ref minLinkInfo);
						IsCloserToPoint (position, br, linkInfo, ref minDistance, ref minLinkInfo);
						IsCloserToPoint (position, (bl + br + tl + tr) * 0.25f, linkInfo, ref minDistance, ref minLinkInfo);
					}
				}
			}

			int linkIdx = Array.IndexOf (text.textInfo.linkInfo, minLinkInfo);
			if (linkIdx >= 0) {
				if (block != null) {
					block (minLinkInfo.GetLinkText (), linkIdx);
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

	static Dictionary<string,TMP_FontAsset> fontAssets = new Dictionary<string, TMP_FontAsset>();

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
	public int lineSpacing = 0;
	public Color fontColor = Color.white;
	public TextAlignmentOptions alignment = TextAlignmentOptions.TopLeft;
	public TextOverflowModes overflowMode = TextOverflowModes.Truncate;

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
			NotificationCenter.postNotification (Scope (), onLinkClick, NotificationCenter.Args ("link", linkText, "linkID", linkID));
		}
	}

	static public TMP_FontAsset GetFont(string path) {
		if (fontAssets.ContainsKey (path)) {
			return fontAssets [path];
		}
			
		TMP_FontAsset font = PlanetUnityOverride.LoadResource(typeof(TMP_FontAsset), path) as TMP_FontAsset;
		if (font != null) {
			fontAssets [path] = font;
		}
		return font;
	}

	public override void gaxb_final(TB.TBXMLElement element, object _parent, Hashtable args) {
		string attrib;

		if (font == null) {
			font = DefaultFont;
		}

		if(element != null){
			attrib = element.GetAttribute ("font");
			if (attrib != null) {
				font = attrib;
			}

			attrib = element.GetAttribute ("onLinkClick");
			if (attrib != null) {
				onLinkClick = PlanetUnityOverride.processString (_parent, attrib);
			}

			attrib = element.GetAttribute ("fontSize");
			if (attrib != null) {
				fontSize = (int)(float.Parse (PlanetUnityOverride.processString (_parent, attrib)));
			}

			attrib = element.GetAttribute ("fontStyle");
			if (attrib != null) {
				fontStyle = attrib;
			}

			attrib = element.GetAttribute ("sizeToFit");
			if (attrib != null) {
				sizeToFit = bool.Parse (PlanetUnityOverride.processString(_parent, attrib));
			}

			attrib = element.GetAttribute ("maxSize");
			if (attrib != null) {
				maxSize = (int)(float.Parse (PlanetUnityOverride.processString(_parent, attrib)));
			}

			attrib = element.GetAttribute ("enableWordWrapping");
			if (attrib != null) {
				enableWordWrapping = bool.Parse (attrib);
			}

			attrib = element.GetAttribute ("maxVisibleLines");
			if (attrib != null) {
				maxVisibleLines = int.Parse (attrib);
			}

			attrib = element.GetAttribute ("minSize");
			if (attrib != null) {
				minSize = (int)(float.Parse (PlanetUnityOverride.processString(_parent, attrib)));
			}

			attrib = element.GetAttribute ("alignment");
			if (attrib != null) {
				alignment = (TextAlignmentOptions)Enum.Parse (typeof(TextAlignmentOptions), attrib);
			}

			attrib = element.GetAttribute ("fontColor");
			if (attrib != null) {
				fontColor = fontColor.PUParse (attrib);
			}

			attrib = element.GetAttribute ("overflowMode");
			if (attrib != null) {
				overflowMode = (TextOverflowModes)Enum.Parse(typeof(TextOverflowModes), attrib);
			}

			attrib = element.GetAttribute ("lineSpacing");
			if (attrib != null) {
				lineSpacing = (int)(float.Parse (PlanetUnityOverride.processString(_parent, attrib)));
			}

			value = element.GetAttribute ("value");
			value = PlanetUnityOverride.processString (_parent, value);
		}

		base.gaxb_final(element, _parent, args);
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
			LeanTween.delayedCall (0, () => {
				if(textGUI != null){
					//textGUI.RegisterGraphicForCanvas();
				}
			});
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
		textGUI.overflowMode = overflowMode;
		//textGUI.extraPadding = true;

		if (maxVisibleLines > 0) {
			textGUI.maxVisibleLines = maxVisibleLines;
		}
		textGUI.enableAutoSizing = sizeToFit;
		textGUI.lineSpacing = lineSpacing;
		textGUI.fontSizeMin = minSize;
		textGUI.fontSizeMax = maxSize;

		//public enum TextAlignmentOptions { TopLeft = 0, Top = 1, TopRight = 2, TopJustified = 3, Left = 4, Center = 5, Right = 6, Justified = 7, BottomLeft = 8, Bottom = 9, BottomRight = 10, BottomJustified = 11, BaselineLeft = 12, Baseline = 13, BaselineRight = 14, BaselineJustified = 15 };
		textGUI.alignment = alignment;

		textGUI.color = fontColor;
		textGUI.text = PlanetUnityStyle.ReplaceStyleTags(value);
	}


	public void SetSizeToFit(float idealNumberfLines) {
		textGUI.enableAutoSizing = true;
		textGUI.fontSizeMax = (rectTransform.rect.height / idealNumberfLines) * 0.75f;
	}

	public Vector2 CalculateTextSize (string text, float maxWidth) {
		return textGUI.GetPreferredValues (text, maxWidth, 0);
	}
}
