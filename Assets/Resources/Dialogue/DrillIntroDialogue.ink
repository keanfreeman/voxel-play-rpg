-> start_knot

== start_knot == 
Haul, one of our beloved miners, is missing. The entrance to the mine appears to have collapsed shut, too. We presume he's stuck in there.
Any questions? -> loop_knot

== loop_knot ==
* [Where are we? What year is it? I feel like I'm losing it.]
    ... Wow. Times are dire indeed. But I'll humor you.
    Your name is Kean and you're our good friend. You live in the house above us in a land of desolate wilderness. You're (usually) brilliant.
    It's no problem though. As long as we've got the mine, our connection to the outside world, and each other, we'll be fine.
    Capiche? -> loop_knot
* [Are the mines dangerous?]
    Usually it's nothing Haul can't handle, but he's been in there for quite a while and hasn't gotten himself out.
    I'm proposing we get in there and make sure he's okay. -> loop_knot
* [How much time do we have?]
    Hard to say, but my gut says we should move quickly. Haul usually would have gotten himself out by now. -> loop_knot
* [No questions here!]
    -> complete_knot

== complete_knot ==
Good. Let's form a search party, open up the mine, and see what's in there. -> END
