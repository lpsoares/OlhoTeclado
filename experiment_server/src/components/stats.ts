import { TrialEvent } from "@/db/event";

export type TrialStats = {
  trialId: number; // Unique identifier for the trial
  duration: number; // Duration in seconds
  typingDuration: number; // Typing duration in seconds
  typingSpeed: number; // Typing speed in words per minute (wpm)
  targetText: string; // Target text for the trial
  finalText: string; // Final text after typing
  minStringDistance: number; // Minimum string distance between target and final text
  editsTarget: Edit[]; // Edits made to the target text
  editsFinal: Edit[]; // Edits made to the final text
};
export function computeTrialStats(
  trialEvents: TrialEvent[] | null,
  trialId: number
): TrialStats {
  const emptyTrialStats: TrialStats = {
    trialId,
    duration: 0,
    typingDuration: 0,
    typingSpeed: 0,
    targetText: "",
    finalText: "",
    minStringDistance: 0,
    editsTarget: [],
    editsFinal: [],
  };

  if (!trialEvents || trialEvents.length === 0) {
    return emptyTrialStats;
  }

  let start: number | undefined;
  let typingStart: number | undefined;
  let end: number | undefined;
  let firstText: string | undefined;
  let finalText: string = "";
  let targetText: string = "";
  for (const event of trialEvents) {
    if (event.type === "TRIAL_START") {
      start = event.timestamp;
      targetText = event.targetSentence;
    } else if (event.type === "TEXT_CHANGE") {
      if (!typingStart && event.text) {
        typingStart = event.timestamp;
        firstText = event.text;
      }
      end = event.timestamp;
      finalText = event.text;
    }
  }

  if (start === undefined || end === undefined) {
    return emptyTrialStats;
  }

  const duration = (end - start) / 1000; // Duration in seconds
  const typingDuration = typingStart ? (end - typingStart) / 1000 : 0; // Typing duration in seconds
  const totalWords = firstText && finalText ? (finalText.length - firstText.length) / 5 : 0;
  const typingSpeed = (totalWords / (typingDuration || 1)) * 60; // Words per minute
  const { distance: minStringDistance, editsA: editsTarget, editsB: editsFinal } = targetText && finalText ? levenshteinDistance(targetText.toLowerCase(), finalText.toLowerCase()) : { distance: 0, editsA: [], editsB: [] };

  return { trialId, duration, typingDuration, typingSpeed, targetText, finalText, minStringDistance, editsTarget, editsFinal };
}

function levenshteinDistance(a: string, b: string): { distance: number, editsA: Edit[], editsB: Edit[] } {
  const distances = Array.from({ length: a.length + 1 }, (_, i) => Array.from({ length: b.length + 1 }, (_, j) => i === 0 ? j : 0));

  for (let i = 1; i <= a.length; i++) {
    for (let j = 1; j <= b.length; j++) {
      if (a[i - 1] === b[j - 1]) {
        distances[i][j] = distances[i - 1][j - 1];
      } else {
        const substitution = distances[i - 1][j - 1] + 1;
        const insertion = distances[i][j - 1] + 1;
        const deletion = distances[i - 1][j] + 1;
        distances[i][j] = Math.min(substitution, insertion, deletion);
      }
    }
  }

  return { ...rebuildEdits(distances), distance: distances[a.length][b.length] };
}

export type Edit = "insertion" | "deletion" | "substitution" | "no_change";

function rebuildEdits(distances: number[][]): { editsA: Edit[], editsB: Edit[] } {
  const editsA: Edit[] = Array.from({ length: distances.length - 1 }, () => "no_change");
  const editsB: Edit[] = Array.from({ length: distances[0].length - 1 }, () => "no_change");
  let i = distances.length - 1;
  let j = distances[0].length - 1;

  while (i > 0 && j > 0) {
    if (distances[i][j] === distances[i - 1][j - 1]) {
      i--;
      j--;
    } else if (distances[i][j] === distances[i - 1][j] + 1) {
      editsA[i - 1] = "deletion";
      i--;
    } else {
      editsB[j - 1] = "insertion";
      j--;
    }
  }

  // Add any remaining edits from either string
  while (i > 0) {
    editsA[i - 1] = "deletion";
    i--;
  }
  while (j > 0) {
    editsB[j - 1] = "insertion";
    j--;
  }

  return { editsA, editsB };
}

