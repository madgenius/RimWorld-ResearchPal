﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace ResearchPal
{
    public static class Def_Extensions
    {
        /// <summary>
        /// hold a cached list of icons per def
        /// </summary>
        private static Dictionary<Def, Texture2D> _cachedDefIcons = new Dictionary<Def, Texture2D>();
        private static Dictionary<Def, Color> _cachedIconColors = new Dictionary<Def, Color>();

        /// <summary>
        /// Get the label, capitalized and given appropriate styling ( bold if def has a helpdef, italic if def has no helpdef but does have description. )
        /// </summary>
        /// <param name="def"></param>
        /// <returns></returns>
        public static string LabelStyled(this Def def)
        {
            if (def.label.NullOrEmpty())
            {
                return string.Empty;
            }
            if (!def.description.NullOrEmpty())
            {
                return "<i>" + def.LabelCap + "</i>";
            }
            return def.LabelCap;
        }

        public static void DrawColouredIcon(this Def def, Rect canvas)
        {
            GUI.color = def.IconColor();
            GUI.DrawTexture(canvas, def.IconTexture(), ScaleMode.ScaleToFit);
            GUI.color = Color.white;
        }

        /// <summary>
        /// Gets an appropriate drawColor for this def. 
        /// Will use a default stuff or DrawColor, if defined.
        /// </summary>
        /// <param name="def"></param>
        /// <returns></returns>
        public static Color IconColor(this Def def)
        {
            // check cache
            if (_cachedIconColors.ContainsKey(def))
            {
                return _cachedIconColors[def];
            }

            // otherwise try to determine icon
            var bdef = def as BuildableDef;
            var tdef = def as ThingDef;
            var pdef = def as PawnKindDef;
            var rdef = def as RecipeDef;

            // get product color for recipes
            if (rdef != null)
            {
                if (!rdef.products.NullOrEmpty())
                {
                    _cachedIconColors.Add(def, rdef.products.First().thingDef.IconColor());
                    return _cachedIconColors[def];
                }
            }

            // get color from final lifestage for pawns
            if (pdef != null)
            {
                _cachedIconColors.Add(def, pdef.lifeStages.Last().bodyGraphicData.color);
                return _cachedIconColors[def];
            }

            if (bdef == null)
            {
                // if we reach this point, def.IconTexture() would return null. Just store and return white to make sure we don't get weird errors down the line.
                _cachedIconColors.Add(def, Color.white);
                return _cachedIconColors[def];
            }

            // built def != listed def
            if (
                (tdef != null) &&
                (tdef.entityDefToBuild != null)
            )
            {
                _cachedIconColors.Add(def, tdef.entityDefToBuild.IconColor());
                return _cachedIconColors[def];
            }

            // graphic.color set?
            if (bdef.graphic != null)
            {
                _cachedIconColors.Add(def, bdef.graphic.color);
                return _cachedIconColors[def];
            }

            // stuff used?
            if (
                (tdef != null) &&
                (tdef.MadeFromStuff)
            )
            {
                ThingDef stuff = GenStuff.DefaultStuffFor(tdef);
                _cachedIconColors.Add(def, stuff.stuffProps.color);
                return _cachedIconColors[def];
            }

            // all else failed.
            _cachedIconColors.Add(def, Color.white);
            return _cachedIconColors[def];
        }

        /// <summary>
        /// Get a texture for the def, where defined. 
        /// </summary>
        /// <param name="def"></param>
        /// <returns></returns>
        public static Texture2D IconTexture(this Def def)
        {
            // check cache
            if (_cachedDefIcons.ContainsKey(def))
            {
                return _cachedDefIcons[def];
            }

            // otherwise try to determine icon
            var bdef = def as BuildableDef;
            var tdef = def as ThingDef;
            var pdef = def as PawnKindDef;
            var rdef = def as RecipeDef;

            // recipes will be passed icon of first product, if defined.
            if (
                (rdef != null) &&
                (!rdef.products.NullOrEmpty())
            )
            {
                _cachedDefIcons.Add(def, rdef.products.First().thingDef.IconTexture());
                return _cachedDefIcons[def];
            }

            // animals need special treatment ( this will still only work for animals, pawns are a whole different can o' worms ).
            if (pdef != null)
            {
                try
                {
                    _cachedDefIcons.Add(def, (pdef.lifeStages.Last().bodyGraphicData.Graphic.MatFront.mainTexture as Texture2D).Crop());
                    return _cachedDefIcons[def];
                }
                catch
                {
                }
            }

            // if not buildable it probably doesn't have an icon.
            if (bdef == null)
            {
                _cachedDefIcons.Add(def, null);
                return null;
            }

            // if def built != def listed.
            if (
                (tdef != null) &&
                (tdef.entityDefToBuild != null)
            )
            {
                _cachedDefIcons.Add(def, tdef.entityDefToBuild.IconTexture().Crop());
                return _cachedDefIcons[def];
            }

            _cachedDefIcons.Add(def, bdef.uiIcon.Crop());
            return bdef.uiIcon.Crop();
        }

    }

}
