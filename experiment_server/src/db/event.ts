export type TrialStartEvent = {
  timestamp: number;
  type: 'TRIAL_START';
  targetSentence: string;
};

export type TrialEndEvent = {
  timestamp: number;
  type: 'TRIAL_END';
  typedSentence: string;
};

export type KeyPressEvent = {
  timestamp: number;
  type: 'KEY_PRESS';
  label: string;
  value: string;
}

export type Context = "Current" | "Previous" | "Next" | "InactiveNext" | "InactivePrevious";

export type Vector2 = {
  x: number;
  y: number;
}

export type Vector3 = {
  x: number;
  y: number;
  z: number;
}

export type KeyPosition = {
  keyName: string;
  keyLabel: string;
  context: Context;
  width: number;
  height: number;
  keyCenter2D: Vector2;
}

export type KeyPositionsEvent = {
  timestamp: number;
  type: 'KEY_POS';
  positions: KeyPosition[];
}

export type ContextPosition = {
  context: Context;
  origin3D: Vector3;
  up3D: Vector3;
  right3D: Vector3;
  forward3D: Vector3;
}

export type ContextPositionsEvent = {
  timestamp: number;
  type: 'CTX_POS';
  positions: ContextPosition[];
}

export type ContextChangeEvent = {
  timestamp: number;
  type: 'CONTEXT_CHANGE';
  originalContext: Context;
  newContext: Context;
}

export type TextChangeEvent = {
  timestamp: number;
  type: 'TEXT_CHANGE';
  text: string;
}

export type GazeEvent = {
  timestamp: number;
  type: 'GAZE';
  gaze2D: Vector2;
  gaze3D: Vector3;
  leftEye3D: Vector3;
  rightEye3D: Vector3;
  leftEyeDirection3D: Vector3;
  rightEyeDirection3D: Vector3;
}

export type TrialEvent =
  | TrialStartEvent
  | TrialEndEvent
  | KeyPressEvent
  | KeyPositionsEvent
  | ContextPositionsEvent
  | ContextChangeEvent
  | TextChangeEvent
  | GazeEvent;

export function parseTrialEvent(rawDataLine: string): TrialEvent | null {
  const [timestampStr, type, rawData] = rawDataLine.split(',');
  const timestamp = parseFloat(timestampStr);
  const dataParts = rawData.split(';').map(part => part.trim());
  switch (type) {
    case 'TRIAL_START':
      return {
        timestamp,
        type,
        targetSentence: rawData,
      };
    case 'TRIAL_END':
      return {
        timestamp,
        type: 'TRIAL_END',
        typedSentence: rawData,
      };
    case 'KEY_PRESS':
      return {
        timestamp,
        type: 'KEY_PRESS',
        label: dataParts[0],
        value: dataParts[1],
      };
    case 'KEY_POS':
      if (dataParts.length % 7 !== 0) {
        throw new Error('Invalid KEY_POS data');
      }
      // Divide in groups of 7
      const keyPositions: KeyPosition[] = [];
      for (let i = 0; i < dataParts.length; i += 7) {
        const [keyName, keyLabel, context, width, height, x, y] = dataParts.slice(i, i + 7);
        keyPositions.push({
          keyName,
          keyLabel,
          context: context as Context,
          width: parseFloat(width),
          height: parseFloat(height),
          keyCenter2D: { x: parseFloat(x), y: parseFloat(y) },
        });
      }
      return {
        timestamp,
        type: 'KEY_POS',
        positions: keyPositions,
      };
    case 'CTX_POS':
      if (dataParts.length % 13 !== 0) {
        throw new Error('Invalid CTX_POS data');
      }
      const contextPositions: ContextPosition[] = [];
      for (let i = 0; i < dataParts.length; i += 13) {
        const [
          context,
          originX, originY, originZ,
          upX, upY, upZ,
          rightX, rightY, rightZ,
          forwardX, forwardY, forwardZ
        ] = dataParts.slice(i, i + 13);
        contextPositions.push({
          context: context as Context,
          origin3D: { x: parseFloat(originX), y: parseFloat(originY), z: parseFloat(originZ) },
          up3D: { x: parseFloat(upX), y: parseFloat(upY), z: parseFloat(upZ) },
          right3D: { x: parseFloat(rightX), y: parseFloat(rightY), z: parseFloat(rightZ) },
          forward3D: { x: parseFloat(forwardX), y: parseFloat(forwardY), z: parseFloat(forwardZ) },
        });
      }
      return {
        timestamp,
        type: 'CTX_POS',
        positions: contextPositions,
      };
    case 'CONTEXT_CHANGE':
      return {
        timestamp,
        type: 'CONTEXT_CHANGE',
        originalContext: dataParts[0] as Context,
        newContext: dataParts[1] as Context,
      };
    case 'TEXT_CHANGE':
      return {
        timestamp,
        type: 'TEXT_CHANGE',
        text: rawData,
      };
    case 'GAZE':
      const [
        gaze2DX, gaze2DY,
        gaze3DX, gaze3DY, gaze3DZ,
        leftEyeX, leftEyeY, leftEyeZ,
        rightEyeX, rightEyeY, rightEyeZ,
        leftEyeDirX, leftEyeDirY, leftEyeDirZ,
        rightEyeDirX, rightEyeDirY, rightEyeDirZ
      ] = dataParts;
      return {
        timestamp,
        type: 'GAZE',
        gaze2D: { x: parseFloat(gaze2DX), y: parseFloat(gaze2DY) },
        gaze3D: { x: parseFloat(gaze3DX), y: parseFloat(gaze3DY), z: parseFloat(gaze3DZ) },
        leftEye3D: { x: parseFloat(leftEyeX), y: parseFloat(leftEyeY), z: parseFloat(leftEyeZ) },
        rightEye3D: { x: parseFloat(rightEyeX), y: parseFloat(rightEyeY), z: parseFloat(rightEyeZ) },
        leftEyeDirection3D: { x: parseFloat(leftEyeDirX), y: parseFloat(leftEyeDirY), z: parseFloat(leftEyeDirZ) },
        rightEyeDirection3D: { x: parseFloat(rightEyeDirX), y: parseFloat(rightEyeDirY), z: parseFloat(rightEyeDirZ) },
      };
    default:
      console.error(`Unknown event type: ${type}`);
      return null;
  }
}

export function parseTrialData(rawData: string): TrialEvent[] {
  const lines = rawData.split('\n').filter((line, idx) => line.trim() !== '' && idx > 0); // Skip header line
  const trialData: TrialEvent[] = [];
  for (const line of lines) {
    const event = parseTrialEvent(line);
    if (event) trialData.push(event);
  }
  return trialData.sort((a, b) => a.timestamp - b.timestamp);
}
