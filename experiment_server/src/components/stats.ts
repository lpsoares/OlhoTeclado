import { TrialEvent } from "@/db/event";

export type TrialStats = {
  duration: number; // Duration in seconds
  typingDuration: number; // Typing duration in seconds
  typingSpeed: number; // Typing speed in words per minute (wpm)
  targetText: string; // Target text for the trial
  finalText: string; // Final text after typing
  minStringDistance: number; // Minimum string distance between target and final text
};
const emptyTrialStats: TrialStats = {
  duration: 0,
  typingDuration: 0,
  typingSpeed: 0,
  targetText: "",
  finalText: "",
  minStringDistance: 0,
};
export function computeTrialStats(
  trialEvents: TrialEvent[]
): TrialStats {
  if (trialEvents.length === 0) {
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
  const minStringDistance = targetText && finalText ? levenshteinDistance(targetText.toLocaleLowerCase(), finalText.toLocaleLowerCase()) : 0;

  return { duration, typingDuration, typingSpeed, targetText, finalText, minStringDistance };
}

function levenshteinDistance(a: string, b: string): number {
  let prevRow = Array.from({ length: b.length + 1 }, (_, j) => j);
  let curRow = Array(b.length + 1).fill(0);

  for (let i = 1; i <= a.length; i++) {
    for (let j = 1; j <= b.length; j++) {
      if (a[i - 1] === b[j - 1]) {
        curRow[j] = prevRow[j - 1];
      } else {
        curRow[j] = Math.min(
          prevRow[j - 1] + 1, // substitution
          Math.min(
            curRow[j - 1] + 1, // insertion
            prevRow[j] + 1, // deletion
          )
        );
      }
    }
    [prevRow, curRow] = [curRow, prevRow];
  }

  return prevRow[b.length];
}
