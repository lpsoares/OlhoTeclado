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
  }, [baseEvents, fps]);

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

  // viewBox is originally a 1.1x1.1 square centered at (0, 0)
  // We need to scale it to 1000x1000
  // Also, the y axis is inverted in SVG, so we need to flip it
  const scaleFactor = 1000 / 0.6;

  const start = trialEvents[0]?.timestamp || 0;
  const currentTime = start + (frame / fps) * 1000; // Convert frame to milliseconds
  let targetText = "";
  let keyPositions: KeyPosition[] = [];
  let gazePositions: Vector2[] = [];
  let curText = "";
  let curCandidates: string[] = [];

  for (const event of trialEvents) {
    if (event.type === "TRIAL_START") {
      targetText = event.targetSentence;
    } else if (event.type === "KEY_POS") {
      keyPositions = getKeyPositions(event, "Current", scaleFactor) || [];
    }

    if (event.timestamp <= currentTime) {
      if (event.type === "GAZE") {
        gazePositions?.push({
          x: event.gaze2D.x * scaleFactor,
          y: event.gaze2D.y * scaleFactor,
        });
      } else if (event.type === "TEXT_CHANGE") {
        gazePositions = []; // Reset gaze positions on text change
        curText = event.text;
      } else if (event.type === "CONTEXT_CHANGE") {
        if (event.newContext === "Current") gazePositions = [];
        else gazePositions = [];
      } else if (event.type === "CANDIDATES") {
        curCandidates = event.candidates;
      }
    }
  }

  let targetTextPos: KeyPosition | undefined;
  for (const keyPos of keyPositions) {
    if (keyPos.keyName.startsWith("Candidate")) {
      const idx = parseInt(keyPos.keyName.split("_")[1], 10) - 1;
      keyPos.keyLabel = curCandidates[idx] || "-";
    }
    if (keyPos.keyName === "textReference") {
      targetTextPos = keyPos;
    }
  }

  const targetTextX = targetTextPos?.keyCenter2D.x || 0;
  const targetTextY = -(targetTextPos?.keyCenter2D.y || -0.1);
  const currentTextPos = keyPositions.find(
    (pos) => pos.keyName === "textField"
  );
  const currentTextX = currentTextPos?.keyCenter2D.x || 0;
  const currentTextY = -(currentTextPos?.keyCenter2D.y || -0.05);

  const viewBox = getViewBox(keyPositions);

  return (
    <svg
      viewBox={viewBox}
      className="w-full h-full bg-white"
      preserveAspectRatio="xMidYMid meet"
    >
      <text x={targetTextX} y={targetTextY} fontSize={24} textAnchor="middle">
        {targetText}
      </text>
      <text x={currentTextX} y={currentTextY} fontSize={24} textAnchor="middle">
        {curText}
      </text>
      {keyPositions.map((keyPos, index) =>
        keyPos.keyName.startsWith("text") ? null : (
          <Key key={`${keyPos.keyName}--${index}`} keyPos={keyPos} />
        )
      )}
      <ScanPath gazePositions={gazePositions} />
    </svg>
  );
}

type KeyProps = {
  keyPos: KeyPosition;
};
function Key({ keyPos }: KeyProps) {
  return (
    <>
      <rect
        x={keyPos.keyCenter2D.x - keyPos.width / 2}
        y={-(keyPos.keyCenter2D.y + keyPos.height / 2)}
        width={keyPos.width}
        height={keyPos.height}
        fill="lightblue"
        stroke="black"
      />
      <text
        x={keyPos.keyCenter2D.x}
        y={-keyPos.keyCenter2D.y}
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
};
function ScanPath({ gazePositions }: ScanPathProps) {
  return (
    <>
      <polyline
        points={gazePositions
          .map(
            (pos) => `${pos.x},${-pos.y}` // Invert y for SVG
          )
          .join(" ")}
        fill="none"
        stroke="red"
        strokeWidth={2}
      />
      {gazePositions.map((pos, index) => (
        <circle
          key={`gaze-${index}`}
          cx={pos.x}
          cy={-pos.y} // Invert y for SVG
          r={5}
          fill="red"
        />
      ))}
    </>
  );
}

function getKeyPositions(
  trialEvent: KeyPositionsEvent,
  context: Context,
  scaleFactor: number
): KeyPosition[] | null {
  return trialEvent.positions
    .filter((position) => position.context === context)
    .map((position) => ({
      ...position,
      keyCenter2D: {
        x: position.keyCenter2D.x * scaleFactor,
        y: position.keyCenter2D.y * scaleFactor,
      },
      width: position.width * scaleFactor,
      height: position.height * scaleFactor,
    }));
}

function getViewBox(keyPositions: KeyPosition[]): string {
  if (keyPositions.length === 0) {
    return "0 0 1000 1000"; // Default viewBox
  }

  const allPositions = keyPositions.map((pos) => pos.keyCenter2D);

  const minX = Math.floor(Math.min(...allPositions.map((pos) => pos.x)));
  const maxX = Math.ceil(Math.max(...allPositions.map((pos) => pos.x)));
  // Invert y for SVG, so we use -pos.y
  const minY = Math.floor(Math.min(...allPositions.map((pos) => -pos.y)));
  const maxY = Math.ceil(Math.max(...allPositions.map((pos) => -pos.y)));

  const padding = 100; // Add some padding around the content
  const width = maxX - minX + padding * 2;
  const height = maxY - minY + padding * 2;

  return `${minX - padding} ${minY - padding} ${width} ${height}`;
}
