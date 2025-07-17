import { Method } from '@/models/method';
import fs from 'fs';
import path from "path";
import { DATA_DIR } from "./database";

export function listSessions(participantId: string, method: Method): number[] | null {
  const participantDir = path.join(DATA_DIR, participantId, method);
  if (!fs.existsSync(participantDir)) {
    return null;
  }
  return fs.readdirSync(participantDir)
    .filter(file => file.startsWith('session-'))
    .map(sessionDir => parseInt(sessionDir.split("-")[1]))
    .sort();
}

export function startSession(participantId: string, method: Method): number {
  const participantDir = path.join(DATA_DIR, participantId, method);
  if (!fs.existsSync(participantDir)) {
    throw new Error(`Participant directory does not exist for participant ${participantId} and method ${method}`);
  }

  const sessionNumber = Math.max(0, ...(listSessions(participantId, method) ?? [])) + 1;
  const sessionDir = path.join(participantDir, getSessionDirname(sessionNumber));

  if (!fs.existsSync(sessionDir)) {
    fs.mkdirSync(sessionDir);
  }

  return sessionNumber;
}

export function getLatestSession(participantId: string, method: Method): number | null {
  const sessions = listSessions(participantId, method);
  if (!sessions?.length) {
    return null;
  }
  const latestSession = Math.max(...sessions);
  return latestSession;
}

export function getSessionDirname(sessionNumber: number): string {
  return `session-${sessionNumber.toString().padStart(2, '0')}`;
}
