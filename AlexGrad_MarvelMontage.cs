
using ScriptPortal.Vegas;
using System;
using System.IO;
using System.Collections;

//
// This script populates the currently selected track (which is naively assumed 
// to be a a video track) with all of the stills from specific folder
// with a quick flipping transition effect and some pan/zoom/rotate that
// approximates some of the flicker/flipping of pictures
// at the start of a Marvel movie
//
public class EntryPoint {

    //
    // How long should each still event be
    //
    const double stillLength=0.166667;
    //
    // How much time should the 'next' still overlap the
    // previous still
    //
    const double overlap=0.0666667;

    //
    // Where are the stills
    //
    const string StillsPath="J:\\AlexVideo\\TitleIntro";

    //
    // Start each still scaled by this (and as we progress forward
    // we will return to unity scale)
    //
    const float initialScale=0.85f;
    
    //
    // Start each still rotated by initialRotationRadians and
    // return to unrotated as we progress forward
    //
    const double initialRotationRadians=-0.0523599;


//
// Returns the currently selected video track. If no video track
// is currently selected, returns null
//
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
    PlugInNode plugIn = vegas.Transitions.FindChildByName("VEGAS Linear Wipe");

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
    key0.RotateBy(initialRotationRadians);


    Effect fx = new Effect(plugIn);
    videoEvent.FadeIn.Transition=fx;
    fx.Preset="Top-Down, Soft Edge";
}


  //
  // FromVegas is always the initial entry point that Vegas makes when
  // calling a script
  //
  public void FromVegas (Vegas vegas) {
      double s=0.0;
      
      //  Gran the media and insert it

      string[] files = Directory.GetFiles(StillsPath);
      for (int i=0;i<files.Length;i++)
      {
          InsertFileAt(vegas, files[i], Timecode.FromSeconds(s));
          s=s+(stillLength-overlap);
      }
  }
}
