// Copyright (c) 2018 Jeffrey Creem
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.


//
// This script populates the currently selected track (which is naively assumed 
// to be a a video track) with all of the stills from specific folder
// with a moderate pace pan/zoom/flip with a little bit of randomness on the
// rotation and panning.
//

using ScriptPortal.Vegas;
using System;
using System.IO;
using System.Collections;
using System.Windows.Forms;


public class EntryPoint {

    //
    // How long should each still event be
    //
    const double stillLength=4.0;

    //
    // How much time should the 'next' still overlap the
    // previous still
    //
    const double overlap=0.25;


    //
    // Start each still scaled by this (and as we progress forward
    // we will return to unity scale)
    //
    const float initialScale=0.85f;
    
    //
    // Start each still rotated by initialRotationRadians and
    // return to unrotated as we progress forward
    //
    const double initialRotationRadians=0.0523599;

    //
    // 
    const string initialFolderRoot="J:\\AlexVideo\\SourcePics";

    //
    //
    string[] desiredTransitions = {"VEGAS Linear Wipe",
    "VEGAS Page Peel", "VEGAS Iris", "VEGAS Portals", "VEGAS Push",
    "VEGAS Star Wipe"};

    Random rnd = new Random();


VideoTrack FindSelectedVideoTrack(Project project) {
    foreach (Track track in project.Tracks) {
        if (track.Selected & (track.MediaType == MediaType.Video)) {
            return (VideoTrack)track;
        }
    }
    return null;
}

//
// Adds a given media fileName to the current track at the specified cursorPosition
//
void InsertFileAt(Vegas vegas, string fileName, Timecode cursorPosition) {
  


    VideoEvent videoEvent = null;

    Media media = new Media(fileName);
    VideoTrack videoTrack = FindSelectedVideoTrack(vegas.Project);
    videoEvent = videoTrack.AddVideoEvent(cursorPosition, Timecode.FromSeconds(stillLength));
    Take  take = videoEvent.AddTake(media.GetVideoStreamByIndex(0));
    videoEvent.MaintainAspectRatio = false;

    VideoMotionKeyframe key1 = new VideoMotionKeyframe(Timecode.FromSeconds(stillLength));
    videoEvent.VideoMotion.Keyframes.Add(key1);
    VideoMotionKeyframe key0 = videoEvent.VideoMotion.Keyframes[0];
    key0.ScaleBy(new VideoMotionVertex(initialScale, initialScale));
    key0.RotateBy(initialRotationRadians * (double)rnd.Next(-1,1));
    key0.MoveBy(new VideoMotionVertex((float)rnd.Next(-15,15), (float)rnd.Next(-20,20)));

    PlugInNode plugIn = vegas.Transitions.FindChildByName(desiredTransitions[rnd.Next(0,desiredTransitions.Length-1)]);

    Effect fx = new Effect(plugIn);
    videoEvent.FadeIn.Transition=fx;

}


  //
  // FromVegas is always the initial entry point that Vegas makes when
  // calling a script
  //
  public void FromVegas (Vegas vegas) {

      Timecode s = vegas.Transport.CursorPosition;
    
      FolderBrowserDialog folderBrowserDialog1 = new FolderBrowserDialog();
      folderBrowserDialog1.SelectedPath=initialFolderRoot;
      DialogResult result=folderBrowserDialog1.ShowDialog();

      if (result == DialogResult.OK)
      {
        string[] files = Directory.GetFiles(folderBrowserDialog1.SelectedPath);
        for (int i=0;i<files.Length;i++)
        {
          InsertFileAt(vegas, files[i], s);
          s=s+Timecode.FromSeconds(stillLength-overlap);
        }

      }
  }
}
