import {
  Context,
  KeyPosition,
  KeyPositionsEvent,
  TrialEvent,
  Vector2,
} from "@/db/event";
import { Player, PlayerRef } from "@remotion/player";
import { useEffect, useRef, useState } from "react";
import { useCurrentFrame } from "remotion";

type TrialVideoProps = {
  trialEvents: TrialEvent[];
  fps?: number; // Optional FPS for video playback
};

export function TrialVideo({
  trialEvents: baseEvents,
  fps = 30,
}: TrialVideoProps) {
  const [trialEvents, setTrialEvents] = useState<TrialEvent[]>(baseEvents);
  const [durationInFrames, setDurationInFrames] = useState<number | null>(null);
  const playerRef = useRef<PlayerRef>(null);

  useEffect(() => {
    setTrialEvents(baseEvents);
    if (baseEvents.length > 0) {
      const firstTimestamp = baseEvents[0].timestamp;
      const lastTimestamp = baseEvents[baseEvents.length - 1].timestamp;
      setDurationInFrames(
        Math.ceil((lastTimestamp - firstTimestamp) / (1000 / fps))
      );
    }
    if (playerRef.current) {
      playerRef.current.seekTo(0);
    }
  }, [baseEvents]);

  if (durationInFrames === null) {
    return <div className="text-red-500">No trial data available.</div>;
  }

  return (
    <div className="w-full h-96 border border-gray-300 rounded-sm overflow-hidden">
      <Player
        ref={playerRef}
        component={TrialComposition}
        inputProps={{ trialEvents, fps }}
        durationInFrames={durationInFrames}
        fps={fps}
        compositionWidth={500}
        compositionHeight={500}
        style={{ width: "100%", height: "100%" }}
        renderMuteButton={() => null} // Hide mute button
        controls
        showPlaybackRateControl
        autoPlay
      />
    </div>
  );
}

type TrialCompositionProps = {
  trialEvents: TrialEvent[];
  fps: number;
};
function TrialComposition({ trialEvents, fps }: TrialCompositionProps) {
  const frame = useCurrentFrame();
  const [keyPositions, setKeyPositions] = useState<KeyPosition[]>([]);
  const [gazePositions, setGazePositions] = useState<Vector2[]>([]);
  const [targetText, setTargetText] = useState<string>("");
  const [currentText, setCurrentText] = useState<string>("");

  useEffect(() => {
    const start = trialEvents[0]?.timestamp || 0;
    const currentTime = start + (frame / fps) * 1000; // Convert frame to milliseconds
    let keyPositions: KeyPosition[] = [];
    let gazePositions: Vector2[] = [];
    let curText = "";

    for (const event of trialEvents) {
      if (event.timestamp > currentTime) {
        break; // Stop processing events after the current time
      }
      if (event.type === "TRIAL_START") {
        setTargetText(event.targetSentence);
      } else if (event.type === "KEY_POS") {
        keyPositions = getKeyPositions(event, "Current") || [];
      } else if (event.type === "GAZE") {
        gazePositions.push(event.gaze2D);
      } else if (event.type === "TEXT_CHANGE") {
        gazePositions = []; // Reset gaze positions on text change
        curText = event.text;
      }
    }
    setKeyPositions(keyPositions);
    setGazePositions(gazePositions);
    setCurrentText(curText);
  }, [trialEvents, frame, fps]);

  // viewBox is originally a 1.1x1.1 square centered at (0, 0)
  // We need to scale it to 1000x1000
  // Also, the y axis is inverted in SVG, so we need to flip it
  const scaleFactor = 1000 / 1.1; // Scale from 1.1 to 1000

  return (
    <svg
      viewBox="-500 -500 1000 1000"
      className="w-full h-full bg-white"
      preserveAspectRatio="xMidYMid meet"
    >
      <text x={0} y={-0.1 * scaleFactor} fontSize={32} textAnchor="middle">
        {targetText}
      </text>
      <text x={0} y={-0.05 * scaleFactor} fontSize={32} textAnchor="middle">
        {currentText}
      </text>
      {keyPositions.map((keyPos, index) => (
        <Key
          key={`${keyPos.keyName}--${index}`}
          keyPos={keyPos}
          scaleFactor={scaleFactor}
        />
      ))}
      <ScanPath gazePositions={gazePositions} scaleFactor={scaleFactor} />
    </svg>
  );
}

type KeyProps = {
  keyPos: KeyPosition;
  scaleFactor: number;
};
function Key({ keyPos, scaleFactor }: KeyProps) {
  return (
    <>
      <rect
        x={(keyPos.keyCenter2D.x - keyPos.width / 2) * scaleFactor}
        y={-((keyPos.keyCenter2D.y + keyPos.height / 2) * scaleFactor)}
        width={keyPos.width * scaleFactor}
        height={keyPos.height * scaleFactor}
        fill="lightblue"
        stroke="black"
      />
      <text
        x={keyPos.keyCenter2D.x * scaleFactor}
        y={-keyPos.keyCenter2D.y * scaleFactor}
        fontSize={16}
        textAnchor="middle"
        dominantBaseline="middle"
      >
        {keyPos.keyLabel}
      </text>
    </>
  );
}

type ScanPathProps = {
  gazePositions: Vector2[];
  scaleFactor: number;
};
function ScanPath({ gazePositions, scaleFactor }: ScanPathProps) {
  return (
    <>
      <polyline
        points={gazePositions
          .map(
            (pos) => `${pos.x * scaleFactor},${-pos.y * scaleFactor}` // Invert y for SVG
          )
          .join(" ")}
        fill="none"
        stroke="red"
        strokeWidth={2}
      />
      {gazePositions.map((pos, index) => (
        <circle
          key={`gaze-${index}`}
          cx={pos.x * scaleFactor}
          cy={-pos.y * scaleFactor} // Invert y for SVG
          r={5}
          fill="red"
        />
      ))}
    </>
  );
}

function getKeyPositions(
  trialEvent: KeyPositionsEvent,
  context: Context
): KeyPosition[] | null {
  return trialEvent.positions.filter(
    (position) => position.context === context
  );
}
