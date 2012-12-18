RavenDB Community Contributions
======================================

These projects are for community contributions to RavenDB, such as extension methods, base classes and helper methods.  Feel free to fork this project, add your own code, and submit a pull request.

This project is maintained and supported by the RavenDB community, not by Hibernating Rhinos.  For questions, please visit the [RavenDB Google Group](http://groups.google.com/group/ravendb).

## Status

We're just getting things started.  Various people are still adding their contributions.

While the code here is probably good, it is currently **unstable**, and various things may change.  If you see something you like, you should probably copy it into your own project, or build the project yourself from source code.  It is too early for a release of any kind, or for a NuGet package.

## Contribution Guidelines

To contribute to this project, [fork](https://help.github.com/articles/fork-a-repo) the main repository [from here](https://github.com/ravendb/ravendb.contrib) and make your changes locally.  Then submit a [pull request](https://help.github.com/articles/using-pull-requests) with your changes.  Be sure to describe what your contribution does and what value it adds to Raven.

Please keep the following in mind:

- We are targeting .Net 4.0, Visual Studio 2012, and RavenDB 2.0-Unstable.

- You should use [xml summary comments](http://msdn.microsoft.com/en-us/library/vstudio/b2s063f7.aspx) on all public members, so that consumers of your code can use intellisense to easily figure out what your contribution does.  *This is especially important for extension methods.*

- Contributions to this project should *augment* the official RavenDB libraries - not *subvert* them.  Pull requests for code that does the exact same thing as something Raven already handles (even if in a slightly different way), will likely be rejected.

- [Hibernating Rhinos](http://hibernatingrhinos.com) retains the right to veto anything we do here.  Raven is their baby.  Let's not make it ugly.  Try to keep their design principles in mind, such as "Safe by Default".

- If something contributed here is eventually pulled in to Raven, it should be deprecated by marking it with an [Obsolete] attribute and replacing the contrib implementation with a call to the new official version.

- If you contribute something to this project, you should be prepared for others to modify it.  You can certainly still make changes, but you are relinquishing control back to the community at large.  If you desire to retain control of your project, you should keep it in your own github repository.

- Everything in this repository is [MIT licensed](https://github.com/ravendb/ravendb.contrib/blob/master/LICENSE.txt).  You may not change the license for any partial contribution.  Unlike [Raven's Contribution Rules](http://ravendb.net/contributing), we do not require a signed contributor license agreement.  By submitting a contribution, you are agreeing to release the code under the MIT license.

- Community member [Matt Johnson](https://github.com/mj1856) is currently moderating this effort.  Matt has agreed to be as non-biased as possible, while attempting to coalesce everyone's contributions into a unified vision.

- If you have a suggestion, opinion, or disagreement, please either create an issue on github [here](https://github.com/ravendb/ravendb.contrib/issues), or discuss in the [RavenDB Google Group](http://groups.google.com/group/ravendb).

## THANKS!