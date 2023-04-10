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
    }
}
