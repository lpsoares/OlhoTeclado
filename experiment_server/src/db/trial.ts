import fs from 'fs';
import path from "path";
import { DATA_DIR } from "./database";
import { getSessionDirname } from './session';

const TRIAL_FILE_HEADER = "timestamp,type,data\n";

export function listTrials(participantId: string, session: number): number[] {
  const sessionDir = path.join(DATA_DIR, participantId, getSessionDirname(session));
  if (!fs.existsSync(sessionDir)) {
    return [];
  }
  return fs.readdirSync(sessionDir)
    .filter(file => file.startsWith('trial-'))
    .map(trialFile => parseInt(trialFile.split("-")[1]))
    .sort();
}

export function startTrial(participantId: string, session: number): number {
  const sessionDir = path.join(DATA_DIR, participantId, getSessionDirname(session));
  if (!fs.existsSync(sessionDir)) {
    throw new Error(`Session directory does not exist for participant ${participantId} and session ${session}`);
  }

  const trialNumber = Math.max(0, ...listTrials(participantId, session)) + 1;
  const trialFile = path.join(sessionDir, getTrialFilename(trialNumber));

  if (!fs.existsSync(trialFile)) {
    fs.writeFileSync(trialFile, TRIAL_FILE_HEADER, 'utf8');
  }

  return trialNumber;
}

export function addTrialData(participantId: string, session: number, trial: number, timestamp: string, type: string, data: string): void {
  const trialFile = path.join(DATA_DIR, participantId, getSessionDirname(session), getTrialFilename(trial));
  if (!fs.existsSync(trialFile)) {
    throw new Error(`Trial file does not exist for participant ${participantId}, session ${session}, trial ${trial}`);
  }
  const line = `${timestamp},${type},${data}\n`;
  fs.appendFileSync(trialFile, line, 'utf8');
}

function getTrialFilename(trialNumber: number): string {
  return `trial-${trialNumber.toString().padStart(3, '0')}.csv`;
}

