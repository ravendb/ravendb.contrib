RavenDB Community Contributions
======================================

These projects are for community contributions to RavenDB, such as extension methods, base classes and helper methods.  Feel free to fork this project, add your own code, and submit a pull request.

This project is maintained and supported by the RavenDB community, not by Hibernating Rhinos.  For questions, please visit the [RavenDB Google Group](http://groups.google.com/group/ravendb).

## Status

The project is relatively **stable**.

## Contribution Guidelines

To contribute to this project, [fork](https://help.github.com/articles/fork-a-repo) the main repository [from here](https://github.com/ravendb/ravendb.contrib) and make your changes locally.  Then submit a [pull request](https://help.github.com/articles/using-pull-requests) with your changes.  Be sure to describe what your contribution does and what value it adds to Raven.

Please keep the following in mind:

- We are using Visual Studio 2012.

- The *master* branch targets RavenDB 2.5, and whenever possible targets *both* .Net 4.0 and 4.5 via linked projects.

- The *2.0* branch is currently maintained for RavenDB 2.0 under .Net 4.0.  If an item is compatibile with RavenDB 2.0, it should be added to both branches.  Of course, code that is not applicable to RavenDB 2.0 will only exist in the *master* branch.

- We are not supporting RavenDB 1.0 at all.

- Whenever possible, libraries should support the current stable release of RavenDB.  However, the minimal supported stable version of RavenDB should be referenced unless their is a breaking change.

- You should use [xml summary comments](http://msdn.microsoft.com/en-us/library/vstudio/b2s063f7.aspx) on all public members, so that consumers of your code can use intellisense to easily figure out what your contribution does.  *This is especially important for extension methods.*

- There are additional restrictions for the `Raven.Client.Contrib` project, as follows:  
** No dependencies on 3rd party libraries or nuget packages.  If you require a dependency, build a separate project.  
** *Must* support the current stable release of RavenDB.  

- Contributions to this repository should *augment* the official RavenDB libraries - not *subvert* them.  A pull request for code that does something Raven already handles (even if in a slightly different way), will likely be rejected.

- Contributions should be small, discrete chunks of code that can operate independently from each other.  If you have a medium to large sized bundle, extension, or demo, then it should probably be hosted in your own repository.

- [Hibernating Rhinos](http://hibernatingrhinos.com) retains the right to veto anything we do here.  Raven is their baby.  Let's not make it ugly.  Try to keep their design principles in mind, such as "Safe by Default".

- If something contributed here is eventually pulled in to Raven, it should be deprecated by marking it with an [Obsolete] attribute and replacing the contrib implementation with a call to the new official version.

- If you contribute something to this project, you should be prepared for others to modify it.  You can certainly still make changes, but you are relinquishing control back to the community at large.  If you desire to retain control of your project, you should keep it in your own github repository.

- Everything in this repository is [MIT licensed](https://github.com/ravendb/ravendb.contrib/blob/master/LICENSE.txt).  You may not change the license for any partial contribution.  Unlike [Raven's Contribution Rules](http://ravendb.net/contributing), we do not require a signed contributor license agreement.  By submitting a contribution, you are agreeing to release the code under the MIT license.

- Community member [Matt Johnson](https://github.com/mj1856) is currently moderating this effort.  Matt has agreed to be as non-biased as possible, while attempting to coalesce everyone's contributions into a unified vision.

- If you have a suggestion, opinion, or disagreement, please either create an issue on github [here](https://github.com/ravendb/ravendb.contrib/issues), or discuss in the [RavenDB Google Group](http://groups.google.com/group/ravendb).

## THANKS!
