namespace MovementDirection {
    public enum Direction {
        NORTH = 0,
        EAST = 1,
        SOUTH = 2,
        WEST = 3
    }

    public static class DirectionCalcs {
        public static float GetDegreesFromDirection(Direction direction) {
            switch (direction) {
                case Direction.EAST:
                    return 90;
                case Direction.SOUTH:
                    return 180;
                case Direction.WEST:
                    return 270;
                default:
                    return 0;
            }
        }

        public static bool isMovingNorth(SpriteMoveDirection moveDirection,
                Direction playerCameraDirection) {
            string enumName = moveDirection.ToString();
            return playerCameraDirection == Direction.NORTH && enumName.StartsWith("FORWARD")
                || playerCameraDirection == Direction.EAST && enumName.StartsWith("LEFT")
                || playerCameraDirection == Direction.SOUTH && enumName.StartsWith("BACK")
                || playerCameraDirection == Direction.WEST && enumName.StartsWith("RIGHT");
        }

        public static bool isMovingEast(SpriteMoveDirection moveDirection,
                Direction playerCameraDirection) {
            string enumName = moveDirection.ToString();
            return playerCameraDirection == Direction.EAST && enumName.StartsWith("FORWARD")
                || playerCameraDirection == Direction.SOUTH && enumName.StartsWith("LEFT")
                || playerCameraDirection == Direction.WEST && enumName.StartsWith("BACK")
                || playerCameraDirection == Direction.NORTH && enumName.StartsWith("RIGHT");
        }

        public static bool isMovingSouth(SpriteMoveDirection moveDirection,
                Direction playerCameraDirection) {
            string enumName = moveDirection.ToString();
            return playerCameraDirection == Direction.SOUTH && enumName.StartsWith("FORWARD")
                || playerCameraDirection == Direction.WEST && enumName.StartsWith("LEFT")
                || playerCameraDirection == Direction.NORTH && enumName.StartsWith("BACK")
                || playerCameraDirection == Direction.EAST && enumName.StartsWith("RIGHT");
        }

        public static bool isMovingWest(SpriteMoveDirection moveDirection,
                Direction playerCameraDirection) {
            string enumName = moveDirection.ToString();
            return playerCameraDirection == Direction.WEST && enumName.StartsWith("FORWARD")
                || playerCameraDirection == Direction.NORTH && enumName.StartsWith("LEFT")
                || playerCameraDirection == Direction.EAST && enumName.StartsWith("BACK")
                || playerCameraDirection == Direction.SOUTH && enumName.StartsWith("RIGHT");
        }

        public static Direction RotateCameraDirection(float direction, Direction currDirection) {
            if (direction > 0) {
                switch (currDirection) {
                    case Direction.NORTH:
                        return Direction.EAST;
                    case Direction.EAST:
                        return Direction.SOUTH;
                    case Direction.SOUTH:
                        return Direction.WEST;
                    case Direction.WEST:
                        return Direction.NORTH;
                    default:
                        throw new System.ArgumentException("Unexpected direction provided");
                }
            }
            else {
                switch (currDirection) {
                    case Direction.NORTH:
                        return Direction.WEST;
                    case Direction.EAST:
                        return Direction.NORTH;
                    case Direction.SOUTH:
                        return Direction.EAST;
                    case Direction.WEST:
                        return Direction.SOUTH;
                    default:
                        throw new System.ArgumentException("Unexpected direction provided");
                }
            }
        }
    }
}
