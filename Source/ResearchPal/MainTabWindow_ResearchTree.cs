﻿using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using RimWorld;
using Verse;

namespace ResearchPal
{
    public class MainTabWindow_ResearchTree : MainTabWindow
    {
        internal static Vector2 _scrollPosition                     = Vector2.zero;
        public static List<Pair<Node, Node>> connections            = new List<Pair<Node, Node>>();
        public static List<Pair<Node, Node>> highlightedConnections = new List<Pair<Node, Node>>();
        public static Dictionary<Rect, List<String>> hubTips        = new Dictionary<Rect, List<string>>();
        public static List<Node> nodes                              = new List<Node>();

        public override Vector2 RequestedTabSize {
            get {
                return new Vector2 (UI.screenWidth, UI.screenHeight);
            }
        }

        public virtual float TabButtonBarPercent
        {
            get
            {
                return 0f;
            }
        }

        public override void PreOpen ()
        {
            base.PreOpen ();

            if (Settings.shouldPause) {
                this.forcePause = Settings.shouldPause;
            }
        }

        public override void DoWindowContents( Rect canvas )
        {
            PrepareTreeForDrawing();
            DrawTree( canvas );
        }

        private void PrepareTreeForDrawing()
        {
            // loop through trees
            foreach ( Tree tree in ResearchTree.Trees )
            {
                foreach ( Node node in tree.Trunk.Concat( tree.Leaves ) )
                {
                    nodes.Add( node );

                    foreach ( Node parent in node.Parents )
                    {
                        connections.Add( new Pair<Node, Node>( node, parent ) );
                    }
                }
            }

            // add orphans
            foreach ( Node node in ResearchTree.Orphans.Leaves )
            {
                nodes.Add( node );

                foreach ( Node parent in node.Parents )
                {
                    connections.Add( new Pair<Node, Node>( node, parent ) );
                }
            }
        }

        public void DrawTree( Rect canvas )
        {
            // get total size of Research Tree
            int maxDepth = 0, totalWidth = 0;

            if ( ResearchTree.Trees.Any() )
            {
                maxDepth = ResearchTree.Trees.Max( tree => tree.MaxDepth );
                totalWidth = ResearchTree.Trees.Sum( tree => tree.Width );
            }

            maxDepth = Math.Max( maxDepth, ResearchTree.Orphans.MaxDepth );
            totalWidth += ResearchTree.Orphans.Width;

            float width = ( maxDepth + 1 ) * ( Settings.NodeSize.x + Settings.NodeMargins.x ); // zero based
            float height = totalWidth * ( Settings.NodeSize.y + Settings.NodeMargins.y );

            // main view rect
            Rect view = new Rect( 0f, 0f, width, height );
            Widgets.BeginScrollView( canvas, ref _scrollPosition, view );
            GUI.BeginGroup( view );

            Text.Anchor = TextAnchor.MiddleCenter;

            // draw regular connections, not done first to better highlight done.
            foreach ( Pair<Node, Node> connection in connections.Where( pair => !pair.Second.Research.IsFinished ) )
            {
                ResearchTree.DrawLine( connection, connection.First.Tree.GreyedColor );
            }

            // draw connections from completed nodes
            foreach ( Pair<Node, Node> connection in connections.Where( pair => pair.Second.Research.IsFinished ) )
            {
                ResearchTree.DrawLine( connection, connection.First.Tree.MediumColor );
            }
            connections.Clear();

            // draw highlight connections on top
            foreach ( Pair<Node, Node> connection in highlightedConnections )
            {
                ResearchTree.DrawLine( connection, GenUI.MouseoverColor, true );
            }
            highlightedConnections.Clear();

            // draw nodes on top of lines
            foreach ( Node node in nodes )
            {
                node.Draw();
            }
            nodes.Clear();

            // register hub tooltips
            foreach ( KeyValuePair<Rect, List<string>> pair in hubTips )
            {
                string text = string.Join( "\n", pair.Value.ToArray() );
                TooltipHandler.TipRegion( pair.Key, text );
            }
            hubTips.Clear();

            // draw Queue labels
            Queue.DrawLabels();

            // reset anchor
            Text.Anchor = TextAnchor.UpperLeft;

            GUI.EndGroup();
            Widgets.EndScrollView();
        }
    }
}