-> start_knot

VAR join_party_request = "none"

== start_knot ==
Hey, it's been a while! Just taking care of a few things, then I'll meet y'all at the Arb.
What's up with you? -> questions_loop

== questions_loop ==
* [Long time no see! Remind me, what do you do around here again?]
    Well, I'm part of the mining operation of course. Somebody's gotta sell all this stuff we dig up. Nobody else wants to do it, so I end up doing a lot of the talking.
    I'm pretty good at it, too! Went to a Bardic college and everything.
    The rest of the time, I'm usually goofing off with Grimes, Corey and Haul. You should spend more time with us down here! I know it's a little dismal underground.
    But it grows on you. -> questions_loop
* [There's so much stuff in here!]
    Yea! Sometimes I like to sit in here and just stare at the gold. It never gets old, for some reason. -> questions_loop
* [Is that a portal back there?]
    Yes, that's how we get people and stuff in and out of here. You could technically walk everything into town, but that's real far!
    And we're a bit short on time these days. -> questions_loop
* [See you at the Arb!]
    Later! -> END
