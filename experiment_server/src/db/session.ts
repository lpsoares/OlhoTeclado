import fs from 'fs';
import path from "path";
import { DATA_DIR } from "./database";

export function listSessions(participantId: string): number[] | null {
  const participantDir = path.join(DATA_DIR, participantId);
  if (!fs.existsSync(participantDir)) {
    return null;
  }
  return fs.readdirSync(participantDir)
    .filter(file => file.startsWith('session-'))
    .map(sessionDir => parseInt(sessionDir.split("-")[1]))
    .sort();
}

export function startSession(participantId: string): number {
  const participantDir = path.join(DATA_DIR, participantId);
  if (!fs.existsSync(participantDir)) {
    throw new Error(`Participant directory does not exist for participant ${participantId}`);
  }

  const sessionNumber = Math.max(0, ...(listSessions(participantId) ?? [])) + 1;
  const sessionDir = path.join(participantDir, getSessionDirname(sessionNumber));

  if (!fs.existsSync(sessionDir)) {
    fs.mkdirSync(sessionDir);
  }

  return sessionNumber;
}

export function getLatestSession(participantId: string): string | null {
  const sessions = listSessions(participantId);
  if (!sessions?.length) {
    return null;
  }
  const latestSession = Math.max(...sessions);
  return path.join(DATA_DIR, participantId, getSessionDirname(latestSession));
}

export function getSessionDirname(sessionNumber: number): string {
  return `session-${sessionNumber.toString().padStart(2, '0')}`;
}
