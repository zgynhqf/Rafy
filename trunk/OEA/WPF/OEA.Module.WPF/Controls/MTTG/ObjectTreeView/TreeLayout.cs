// hardcodet.net WPF TreeView control
// Copyright (c) 2008 Philipp Sumi, Evolve Software Technologies
// Contact and Information: http://www.hardcodet.net
//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the Code Project Open License (CPOL);
// either version 1.0 of the License, or (at your option) any later
// version.
// 
// This software is provided "AS IS" with no warranties of any kind.
// The entire risk arising out of the use or performance of the software
// and source code is with you.
//
// THIS COPYRIGHT NOTICE MAY NOT BE REMOVED FROM THIS FILE.


using System.Collections.Generic;

namespace Hardcodet.Wpf.GenericTreeView
{
  /// <summary>
  /// Encapsulates the basic layout (selected / expanded nodes)
  /// of a tree control.
  /// </summary>
  public class TreeLayout
  {
    /// <summary>
    /// The ID of the selected item, if any. Defaults to null
    /// (no node is selected).
    /// </summary>
    private string selectedItemId = null;

    /// <summary>
    /// A list of expanded nodes.
    /// </summary>
    private readonly List<string> expandedNodeIds = new List<string>();

    /// <summary>
    /// The selected group item.
    /// </summary>
    public string SelectedItemId
    {
      get { return selectedItemId; }
      set { selectedItemId = value; }
    }


    /// <summary>
    /// A list of expanded nodes.
    /// </summary>
    public List<string> ExpandedNodeIds
    {
      get { return expandedNodeIds; }
    }


    /// <summary>
    /// Checks whether a given node is supposed to be
    /// expanded or not.
    /// </summary>
    /// <param name="nodeId">The ID of the processed node.</param>
    /// <returns>True if <paramref name="nodeId"/> is contained
    /// in the list of expanded nodes.</returns>
    public bool IsNodeExpanded(string nodeId)
    {
      return expandedNodeIds.Contains(nodeId);
    }
  }
}