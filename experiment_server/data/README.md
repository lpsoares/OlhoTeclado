# Experiment Data

## Schema

The experiment data is structured in a CSV format with the following columns:

- `timestamp`: The time when the event occurred, in milliseconds since the start of the session.
- `type`: The type of event, such as `KEY_PRESS` for key presses (see [`data`](#data) for details).
- `data`: The data associated with the event, which varies based on the `type`.

## Data

The `data` field contains different information depending on the `type` of event. Information is separated by semicolons (`;`) when multiple pieces of data are present.

### Trial Start

The start of the trial is recorded with the `TRIAL_START` type. The `data` field contains the following information:

- The target sentence for the trial.

### Trial End

The end of the trial is recorded with the `TRIAL_END` type. The `data` field contains the following information:

- The sentence that was typed during the trial.

### Key Presses

Key presses are recorded with the `KEY_PRESS` type. The `data` field contains the following information:

- The key name;
- The key label of the pressed key;
- The key value of the pressed key.

### Key Positions

Key positions are recorded with the `KEY_POS` type. The `data` field contains one or more occurrences of the following information:

- The name of the key (unique);
- The key label;
- The context that contains the key;
- The width of the key;
- The height of the key;
- The x 2D coordinate of the key center on the context plane;
- The y 2D coordinate of the key center on the context plane.

### Context Positions

The positions of the context planes are recorded with the `CTX_POS` type. The `data` field contains one or more of the following information:

- The context name (e.g., `CURRENT`, `NEXT`, etc.);
- The x 3D coordinate of the context plane origin;
- The y 3D coordinate of the context plane origin;
- The z 3D coordinate of the context plane origin;
- The x coordinate of the up 3D vector;
- The y coordinate of the up 3D vector;
- The z coordinate of the up 3D vector;
- The x coordinate of the right 3D vector;
- The y coordinate of the right 3D vector;
- The z coordinate of the right 3D vector;
- The x coordinate of the forward 3D vector;
- The y coordinate of the forward 3D vector;
- The z coordinate of the forward 3D vector.

### Context Changes

Context changes are recorded with the `CONTEXT_CHANGE` type. The `data` field contains the following information:

- The original context (e.g., `CURRENT`, `NEXT`, etc.);
- The new context (e.g., `CURRENT`, `NEXT`, etc.);

### Text Changes

Text changes are recorded with the `TEXT_CHANGE` type. The `data` field contains the following information:

- The new text after the change.

### Candidate List

This event indicates that the candidate list has changed. It is recorded with the `CANDIDATES` type. The `data` field contains the candidates separated by semicolons (`;`).

### Gaze Data

Gaze data is recorded with the `GAZE` type. The `data` field contains the following information:

- The x 2D coordinate of the gaze point on the keyboard plane;
- The y 2D coordinate of the gaze point on the keyboard plane;
- The x 3D coordinate of the gaze point in the 3D space;
- The y 3D coordinate of the gaze point in the 3D space;
- The z 3D coordinate of the gaze point in the 3D space;
- The x 3D coordinate of the left eye in the 3D space;
- The y 3D coordinate of the left eye in the 3D space;
- The z 3D coordinate of the left eye in the 3D space;
- The x 3D coordinate of the right eye in the 3D space;
- The y 3D coordinate of the right eye in the 3D space;
- The z 3D coordinate of the right eye in the 3D space;
- The x 3D coordinate of the direction of the left eye;
- The y 3D coordinate of the direction of the left eye;
- The z 3D coordinate of the direction of the left eye;
- The x 3D coordinate of the direction of the right eye;
- The y 3D coordinate of the direction of the right eye;
- The z 3D coordinate of the direction of the right eye.
