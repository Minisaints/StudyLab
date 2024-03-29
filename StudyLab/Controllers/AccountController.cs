﻿using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using StudyLab.Models;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace StudyLab.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public AccountController()
        {
        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager {
            get {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager {
            get {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set {
                _userManager = value;
            }
        }

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;

            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true
            var result = await SignInManager.PasswordSignInAsync(model.Username, model.Password, model.RememberMe, shouldLockout: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid login attempt.");
                    return View(model);
            }
        }

        //
        // GET: /Account/VerifyCode
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            // Require that the user has already logged in via username/password or external login
            if (!await SignInManager.HasBeenVerifiedAsync())
            {
                return View("Error");
            }
            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // The following code protects for brute force attacks against the two factor codes. 
            // If a user enters incorrect codes for a specified amount of time then the user account 
            // will be locked out for a specified amount of time. 
            // You can configure the account lockout settings in IdentityConfig
            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent: model.RememberMe, rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid code.");
                    return View(model);
            }
        }

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Set default avatar when new account is registered
                const string defaultAvatar = "iVBORw0KGgoAAAANSUhEUgAAAZAAAAGQCAQAAAAqtv5HAAAACXBIWXMAABcRAAAXEQHKJvM/AAAAB3RJTUUH4QQEDjc2Pg1LtwAAIyxJREFUeNrtndtPXVXXxidtrW0R2wataRRJ9MJDGuJVq0YNiVGTaqLGQ2q8kBrb+EZvvPPCRNNommiiiRcmDxTK0UIt8rZY/GhLqAWkL6fSjx6+WkLBDfQVKKdy2LCB57vAyqF7s9faxzXnGs/4C9bc47fnHHMcplKiGAjrkYIn8DzexF58hu+QgyOoRB1acRUe9GMEE5jGLAhiFtOYwAj64cFVtKIOlTiCHHyHz7AXb+J5PIEUrJdVFemKw2rchzS8gAx8iVycQBv6MIpxeP+BIFSbxTS8GMco+tCGE8jFl8jAC0jDfVgtKy9yLhSJeAhP4V18hWI0wIMBjMEXFgxWzYcxDMCDBhTjK7yLp/AQEuUXEcUfilVIxmPYiU+Rh0Z0YhAzMUFiJZvBIDrRiDx8ip14DMlYJb+UKJZYKNyFB5GOT3AQLejGVNyhCGRT6EYLDuITpONB3AX58URR3S+SkIJX8QVOoh2TjsXCn02iHSfxBV5FCpJkTxFFEowErMU9eBGfoxo9WmHhz3pQjc/xIu7BWiTIrysKQ2sUFNLwMcrRgznt0Vhsc+hBOT5GGtQa+aFFIewbyXgJ36PNKCz8WRu+x0tIll9cZBWNVLyNAlyD17BdI/Bu4sU1FOBtpMqvL1opCE/FLhTBo1kAHrlA3oMi7EKqBPGi5WhsxWvI0u5uKlr3XVl4DVsFE5HaoLAJT2M/WjHmejQW2xhasR9PY5P4iHv3jbVIxR5UYViACGDDqMIepGKteIu70EhAIp7FD+gQCCxYB37As0iUrIkrtE5hEzJwGl5xfRvmxWlkYNM6cSDDd47H8TU84vAhmgdf43HZScyE406koxADYfZjuN1mMYBCpONO8SiT4NiMN1COEZck/qKfWBxBOd7AZvEs7ZWosAUZqMKoOHaEbRRVyMAW8TGdI44tyEAdJsSdo2QTqEMGtkhUoiMc9+I91Dq4mckUm0It3sO9AolOcGzEO6gROGIISQ3ewUaBRAc8VmMnTmNa3DbGNo3T2CmTVZwNxyo8iVMOGJzgVpvBKTwpRY5O3TnSkI0RcdM42wiykSY7idPwSMU+dEkS0CHJxC7sk+Yr58CxEbvRHKMhbWJWh9k1Yzc2infGG4478AzKpJfDoT0lZXgGd4iXxu86937sQ5+4ooOtD/twv1z/xgOPDXgdTeKCGlgTXscG8dgYaoPCw8jFuDifJjaOXDwsjMRu79iDTnE7zawTe2Qfibo2K2zDUZk9oum8lKPYJj4czb0jCR+gQ7o6NO4k6cAHSBJPjk4ZyaPIl9J1A0rk8/GolKNEGo/12IVW2TsM2UdasUveWIwkHg/gG5leZZQN4xs8IJ4dmaPVdpyRQhIDi1HOYLsctcJSgkIC9qJX3MlQ68VeJEiaPfTdYysyxY0Mt0xsFU8P7Wi1A/VSwO6C4vh67JCjll081mG3TM51jXVgN2SqqQ08krEfN8RxX" +
                                             "GQ3sF+egbOKxyMolokkrrMpFOMR8X4rl7pnJfJwaTRyVi5+V8bjDrxiwEvkYqFbD16RHsRAeKzFRxgSJ3G5DeEjedHKHx5341u3PGqTyQKWsZJ1bGM7u9nPEU5wmrMkyVlOc4Ij7Gc329nGOlayjAXMdAsiXnyLu4WIpXikoMj0gW8HWMIKtrCDg5yij7OcoxXNcZY+TnGQHWxhBUt4wHREZlCEFKFiAY9tqDAXjwM8zCpeZB8n6WO48nGSfbzIKh42GZQZVEhz1S08dqDWTDxyWcZGdnOcM4y0ZjjObjayjLmmIlKLHa6HI0nhObSZ1+eRw1I28DonGW1N8jobWMocE/tG2vCc27Me6eg2LQAvZDW7YoDGUky6WM1C8wL5bqS7NjOCNXgZf5n0c2axhOc4bDH4jrTmOMxzLGGWWYj8hZexxo14rMZbGDBp5yjjFU4x3priFZaZtZMM4C3XTYvHHXgfg+bsHMd4LQI3VJGSj9d4zKSdZBDvuyq/jrX40JTdI5NlbHfAznH7TtJu0k4ygA9dk1/HWvzLlJHTh3ghxuG4vdD9Ag+Zgkgf/uUKRHAHPjQDjzzWcChOAbn1wH2INcwzBZEPjT9oYTXeN+FwlcVj7IpC+i8ammGXKRHJAN43OlzHGrxlQmiezwaOUyeNs4H5ZoTrbxl76YtVeFn/3SOTpfT8XXerk2bpYakJQfsAXjYydZikkG5CWvA3jlFXjfE3M1KH6QbOv8Zz+heVFPASddclFphQgGJajRZ2oE33n6WUvQ6/s7J2r9XLUv0RaTOq0hfbUKt3xW4WKzlMUzTMSt1vteZQa0y/CFJ0b4c6wFqNIw//0Uit7u1WM6gwousQd+veTJvNJnppmrxsYrb+jbm6965jLb7VG48cntckIWg/gXhe9zarGXyrdfkJ7sBHek8qyeZFAwLzwAH7Rd13ES8+0rb8BKvwit5zrrJ5gabrgu6IDOEVTROH2K73lMQcXqQbdFH3g1YPtuuIxyM4q/fucd7gw9XSg9Z53XeRs9qNvUYyinUeQX2ATYaG5v7D9Sa9L31nUazV4wlYh/06P2CQxVoDL3ZXvvSt1Tt1OIX92jzBg1XYrffzN5WGpQWtpQ4r9T5m3cBuTYJ17ND78bRSg4pK7BWgaF6j1aFBfVaCwlbU612x20u3qlf3St96x7+YiwRk6v0+1CWX3F35v8+6pDcgs8hEgrOjj716dwv+RrfrN927Dvc6OBLBdvTqHX2MuR6QMd0jkV7Hpg3xAM7oPYrBQxHp0X28wxk84EQ81uMb+HTOfTRoOIohGpplg945ER++wXrnRR+7MKzz/84xzQb5RFPjPKb3HjKMXQ6LRPAoWnVe0jx2CReL1KX7NMZWPOokPJKQr3fPeY2LKq+sVWfV6N6zng+nzAbarPABJnRezkMcEiaWaUj3sdcT+GCzY2aWaF1akskLLk4OBk4aXtA9I9LhiLkn2ICjeh+vyhz8gEE8Ncky3Y9ZR7EhznhsUNiDSZ2XMYvtwkIAtes+PWsSe+K9fzyMTq2XkMc" +
                                             "c+DqUUzSl+3Uv0YmH43u8ytV7ATN5TThYQdf0nwefG7djFhLwOsb1Xr4yBz296UT5dI9DiHG8HqcKX9yPJr0XL4tXhIEguqL/21RNuD8+Q+H2ab5wLJH4w0IcUqI7IMS+OAyXwzO6P8WZyXPi/xZ0Tv84pA/PxBqPjSjT/X+l0KW953Y1zEL995AybIwtILsxpvuiVUv+3GJOvVp/QMawO5Z4pKJZ9yXLkfpdG7W9Ofoj0ozUWOGxGvt0bo261V4rBSbWi04MeLTNh30xemMdaejSfrnYIH5vQw36A0J0IS02vYPZeg/2AcFcXhevt6HrzNUfkFlkx6DPEE9iRP9/E6ngdVll77yN4Mnoxx+nDFgoNorP21SjCYAQp6IahyABO/V+cXDeDrBbPN6munV/FXfeZrAzipVZ2IjTJvyPHJYJJrY1zsNm7CGno5YyRALewbQJi1QlIxpsa4ZVZgAyjXeitIfgXtQYsUQueXsw0rpoBiBEDe6Nzv7xns6vRi2OQPrE20NQnxlRCDGF96Kwh2ALas34BymRK94Qr3pLTNlDarElwngkKmSYsX+AFdJFGJJ8rDAFkClkJEZ8/6gzZHHYIr4eolpMAYSoi/Aeggy9Zycutg7x9BDVYQ4gE8iIJB6bUWXK0mRyUDw9RA3q31u4YFWI3GRSvIFRUxamQPrQQ9aU7o98LrZRvBEpPO5EuTHLIoN+wgrTy8wBhCjHnZEBJN2E+t1bVinvSIWsWVaaBMgI0iOTICzUezj1UquTTvSQNcc6kwCZQ2EEEoZ4HAMGLQrbxM/DUJtJgBADeDxMPNYpfK1//+Bik1nu4ajdLEBm8XW4+8cmeIxaEukECUvdZgFCeLApvPgjw7AFYb94eRjqNw0QIiOMOASJZjRILbYR8fIwNGIeIKcRelkWnoXXtAWZEC8PQxPmAeLFs6HisRY/GLccnBYvD0PT5gFC/IC1oQ4Y7TBvOSRNGF6q0EBAOkIaSrpBYY+BiyE+HqZM9Ans2RDSBW+VACJyBSBVIVz24mkMm7gYcsSSI9ZtNoyn7U/g3W/kUkiQLkG6P9tvc2ovtqLVzKWQa1655vVjrdhqD5DX9H9BShKFkii0bGN4zd4BK8vQhZBSEyk18W9ZNg5ZSEW7qQshxYrhqNtcQNptZEOwC5OmLoSUu4ejdnMBmcQu64AUGbsM0jAVltrMBYQosn7A8pi7DNJyG7oMa7m9vTfE2iELb5t7wJKhDeGlCStNBmQSb1sDpMDgRZCxP2HIsLE/t1uBFTyScc3kRZDBcaHLqMFx/uwakoMD8pJ5TVKLTUaPhi6jRo/6b556KQgeaxS+N2kKlj+T4dWhqsNsPIg5fB9s/1BoM3wR5PmDkNViOiBEG4IAkmb8EsgDOiGH6BXmA0KkrTzm52Pzl0CeYAtNBj3BtpJ9vMIYIKw1aY57IJNHPEOTMY94rmzlK4xwwD3occESyDPQIemiG/AgenBPYEBeNP0Ga96qOCP+blMzrHIHIHN4MXAXyOeuWAIe5rh4vE2N87A7ACE+D9AZgiRUu2MJDkhXiG11uyMCIYhq" +
                                             "JPkHJMUdEQgINorH21SjW/AgepDiH5BXXbMELJOrXptXvGXuAYR41X8O/Qv3LEEur4vX29B15roJkC/85NNxF066aAnYIF5vQw1uwoM4ibtuB+RBcwc1+LNSOWTZOGCVuguQdjzo77HnSTctQg67xPMtqos57gJk0s8D0fgE7loEVkt3uiXNsZpu8w18cnuS8KDbFqGQw+L9FjTMQvcBcnBZshDJaHHbImTynHi/BZ0zvY/Qn7Usa77FY+h23SKwRPrTg2rKHUXuy60bjy0FZCem3LcMWbwiBATRFWa5EZAp7FwKyKdw4zLIEKAg8rkrg77YPl0KSJ47lyGT14SCFXTNjfHHvOUtxiMRjS5dBh6TOGSF+OOYW/EgGpG4AMhD6HTrQmTJvPeAandn/DFvnXhoAZCnMOjahZDKXqng9WeDeGoBkHcx496lyOQFyan7yZ9fcG/8QRAzeHcBkK/g5qXgIQ4JEcs0xEN0t1fgqwVAil2+FKyRMQ5LNMMat+NBFN/CYzUa3L4YeVLbu6x+N08AacDqeUDuM/lFKevXvTLp5JbG3Xy9u2Ae3HdrHu+ALEcWG+TtKZLkLBvcfL27YAN/z+nFCxiT5QDz6RE6SHqYL95AEGN4YR6QDPhkOUCwlGOux2PMbe21gc2HjHlAvpTFuJUR+c31gPzm7uzHUvtyHpBcWYoFu+TipOEcL4kHLLbceUBOyFIsWAF7XQtIr+mPdNq1E/OAtMlSLI1E3NmrPizRx3JrU0phPfpkKZZapQuD9TFWyi+/3PqwXiEFo7IUy3MitfS6Cg8vayX3cbuNIkXhCYzLUiy3A2xyUXXWDJvc87iBHRvHEwrPwytLcbtl87xL7rPmeJ7Z8ov7My+eV3gT07IU/izHJW8ZXnTbYFHrNo03FfZiVpYi0C5ywXg8LsjuEdhmsVfhM1mIlRC5aPBBa44XBY+V7TOF72QZVj5onTc0XJ/heTlcBbPvFHJkGYLtIk0GXvp62SS7R3DLUTgiyxD80rfWsNThGGvlYteKHVGolGWwkjqsNKgAZZiVkha0ZpUKdbIMVmu0eg0I2OfYKzVX1q1OoVWWwXql7yXtAbkkFbt2rFXhqiyDHftN42hkjL/JL2jPriqZaGLPMllKj4bjHWbpYal0C9o1j0K/LINdy2eDZkOCxtkgoxhCsX6FEVmGUG61jrFLkwTiDLt4TG6tQrMRhQlZhtAsjzUccvi91hyHWCNTEkO3CSW1vOHYIV5w8OMJk7wgI6jDs2kltbzhBu1lbHfgK1VTbGeZBOXh2qySRYhMRHLNQc+B+nhNoo4ImQASwZ3kigN2kilekZ0jooDIESuCO0kJz3E4ToH7HId5jiWyc0T4iCVBeoR3kkJWsy" +
                                             "vGofsku1jNQtk5ohCkyzVvFCyHpWzg9RhgMsnrbGCptD5F7ZpXEoVRs1yWsZHdHI9CSnGG4+xmI8uYKysd1UShlJpE2Q7wMKt4kX2cjMBNl4+T7ONFVvGwtDzFpNREihVjBkoJK9jCDg5yij7OWgzm5zhLH6c4yA62sIIlAkbszCPl7nEJ5AtYxkrWsY3t7GY/RzjB6b9rhGc5zQmOsJ/dbGcb61jJMhZIAB4PuyoNU2Jiga1VWm7FxAJbnQxtEBMLbJUy9kdMLLAdkcFxYmKBLUdGj4qJBbbvZHi1mFhg+0yePxATC2Sz2CsP6IiJBbJpvClPsImJBTIvnpdHPMXEAtk4npBnoMXEAtkoUhTWo0+WQkzMj/VhvVIKbbIUYmJ+rE0ppRROyFKIifmxE/OA5MpSiIn5sdx5QL6UpfBvB/kzT/J3/i+v0sN+jnKSPo1fmZqjj5McZT89vMr/5e88yZ95UH7pQPblPCAZ8MliLFghj7Oef3Dg78ZYkzXfzDvAP1jP4yyUX3+x+ZAxD8gLGJPlAAt4nM28znEHDRGN7cDScV5nM4/LI23zNoYX5gFJw4CbFyKTP/Ik/+CwS8G4HZRhXuUpHnJ7F/wA0uYBuc+9k03y+Sv/4LDhB6nQ4pUR/sFf3fwulQf3zQOyGg3u+/wslrCR/QY87BxdTPrZ6NZ5vw1YreaFYnd9+gH+m1foFf+3KC+v8N/um8ZVrG4JX7npw8vZJUeqEO67uljuLkC+WgDkXcy4Ixw/yh45VIVx4OrhUbeE7jN4dwGQpzBo/icfYbsm79I6+83cdh5xAyCDeGoBkIfQaXqOo4UT4t0R0gRbzM+VdOKhBUAS0WhySF4pt1VRuN06aXbY3ohEtSDkmfqhRbzMafHoKGial1lkLiB5arHwqZm5juMcEk+OooZ43NQcyadLAdmJKfNqcZukeCQGhSlNJtYDT2HnUkAeQ7dZn/gjPeK9MdKf5h21uvHYUkCS0WLSB1bwpvhtDDXK42YB0oLkpYCswkFz0oF1UkYSh3KUOpNSiAexSi0VPjHj07LZKpFHnKKRVmabAsgnarmQjkn9PyyHl6XOKm6a5WUz3mufRPrtgDyIdt0/LJdXxEvjrD+Ypz8g7XjwdkDuwkndm5+uin86QO36N1mdxF23A6Lwhc4flcd28U3HIKL5LvIFlB/hVZ1jjz/ELx110NI6FnlV+QUkBT26liNeFp90mC7rW8rYgxT/gCShWs+8xzm5uXLgjdY5XfMi1UjyD8gqfK7jB9VJ3sOheZE6PQH5/LYk4T+IvIg53T7nuGTNHZxd17AAZQ4vqkDCPbpFIUUcFT90sEb1K2PswT2BAVmLcr0K2v8UH3S4/tStGL4cawMDkoCPdWqHahb/00DNerVUfYwEFVhI06mkXYJzPYL1Cp0ASVMrCdo8yFYkzbTaaEifSKQNK/Kh1ih8r8NNlqQGJW0YlRus71Uw4SV4nf8plTKpRCtNs1IHQLx4KTggybjm9A8pYL/4nGbq12HU3LVljbYBEClw+oe0yBg47TTHFucDUqCsCG87u7fwiAwR" +
                                             "1VITTp/pO4m3rQGS6uQXpzKl70NbtTu7fNGDVGVNKHLuZxyVCe3aaoZHnQxIkbIq7HLuIatH/Exj9Tj5gLXLOiCpTh3gUC7hueahumNfqWq3fMBSCquQ5cz0YKf4mObqcmrKMCtgF4hfRF7DmPM+4meJP7TXLP/tRDzG8JqyI2xFq/Oqd6W8xARdcWJ1byu22gNkFfY77SOKpXfQCHlZ7DxA9ts6YCmlFJ7GsLM+olF8yxA1Og2PYTyt7AqbUOWkj8iX+itjNOC02YtV2KTsC3uc9BG/ygWvQZe9vzoLkD0qFCEVHc4pMJHJiSbpDycVnXTYyIAsG+Hwg1M+4hBHxKsM0jB/dA4gP6wwpiEIIs86pXnqlBywDMuGnHROk9SzKlQhEaed8RnysIFpuuoUQE4jMXRAEpDhjA7CYfEo4w5ZDrnJylhxzI+Fy14H9IYclwE/xmmavzijB2STCkfrFL7GbLw/QwbEmaim+OMxi69VuMLjGIj3h1wXbzJQ1+MPyAAeDx+QBBTGd1JWAcfEmwzUWLznnMyhMKz4Y9ED0SMSgYgiLV+8H0cY8fPYc0iA3Bnfme9nxZcM1dn4AlKOO1VkhDcwGr8PkSkmpqo9nniM4g0VKWFzPCt7B8WTDNVgfCt4N6vICRmYiM+H5HJKPMlQTTE3XnhMIENFUtiCunj1oUuIbm6Y/nO8AKnDFhVZIQNT8fiUk/LEs7GKW8niVIT3j7/3kNp4fMzv4kcG6/f4AFIb8f1DKSTgvXjsIefFiwzW+fjsH+9FJEF4GyL3okYK3UWRVFyK3mtwr4qGkIB3MB3rz/GIFxksT+zxmMY7Udk/lFIKG2PfQCWzTExWfzwapDaqaAkJ2ImZ2H7QqHiRwRqNNR4z2Bm1/UMppbAap2L7STJN0WR5Yw3IKaxW0RWejG1tr6QJTZYv1vW7T6poC6uQHcseQ5lmYrLmYts/mG17Am9IiKShK3afJTJbMQSkC2kqFsJq7INPABFpBYgP+6IefywaStosgIi0AqQ5xAGjISKyO1YvUIkEkAjYGHarWAobUSaAiLQBpCyK6cEAiDyDPgFEpAUgfXhGxVq4A/sEEJEWgOzDHSr2wv1oEkBEjgekCfereAgJeB3jAojI0YCM4/WoVl+tiMgG5AogIkcDkosNKn7Cw+gUQESOBaQTD6t4aoPCHkwKICJHAjIZ4vOcET5mHY3mcGuRABLycOqjcT1e/YPItmi+hisSQEK0DmxTzhA+iN7cRZEAEuLsxA+UU4Qk5EfrmCUSQEI6XuUjSTlHeBStAojIMYC04lHlJGEVdmFYABE5ApBh7IpJ76AtRNbjm2i0UYkEENutUd9gvXKe8ADOCCCiuANyBg8oZwrb0SuAiOIKSC+2K6cKq7BXABHFFZC9jos+llX4ZkZ2KJBIALEx2CczbpW71pSgsBX1AogoLoDUY6tyvrAjkqUnIgHEcmnJDqWDsAq7cUMAEcUUkBvY7ejoYwki67A/Uq9RiQQQS69G7cc6pY+QjOLIBOsiAcRCcF6MZKWX8AjOCiCimA" +
                                             "ByFo8o/YTt6BFARFEHpMfBqcEgwforGBJARFEFZAivaBOc+xku9xG8AogoaoB48VFchsJFDJG1+Da8Vw1FAsgKLw5+i7VKb+FuFIWDiEgACYhHEe5W+gspqAgdEZEAEgCPCqQoM4RtqA21Z10kgPjtOa91zMySCNVntQkgoogB0qZJ3ZUNRJ5DtwAiiggg3XhOmSek4y8BRBQ2IH8hXZkorMLLGBBARGEBMoCXtU0MBkVkDd7CoAAiChmQQbyFNcpcYTXet7eLiASQRbvH+zF76zyO5Scf2nn+UySA/PMU54dal5XYKD/5l3VERALI33j8S/uyEhuIfGj1oCUSQEAM4EPX4PH3Qet9a+G6SADBIN53xeFqWbj+lpVdROR6QAbwlvGheYBL35eDpw4vcEa8yFDN8IKVtODLRl/sBkkdpgcrQDnAWo6JLxmoMdbyQPCiknRj04JWlKTwHNpWrvTN4nH2c048yiDNsZ/HmRWsYrfNyJqrECp9a4P1i5Twqhy1DDpaXWVJ8H6PWuMqdsPoFwnaUpXLBk6IbxmgCTYw10o71DYhYwGRlOCNuVms4KActTQ/Wg2yItjRar6ZNkWoWIrI3fg2+ASUArYLIhrj0c4CK5NKvjWi1zwK+fWPgs/RyuIZ3hRf01A3eSb43kEM4SNX5cxt5tdfCT6NMZOl/JOz4nEaaZZ/spSZVqYkvuK6nLnNzMh2nA0+9jqPZzkufqeJxnmWeVZGUJ/FdldnPSxC8giKgz+ekMVydss+osHe0c1yK0erKRRrOYI6LogkY7+VJ3jy+R9Oig86WJP8D/OtPX+zX7sHDOKKyDrstvaQ2zF6JIXo0HSgh8esPp62W6vnbxwSjexAvZVHeA6ynjfl8tdhF7o3Wc+D1h6/qccOiTxCg2QrMq011ByWUhRH7R1XedhqK1SmFi/TOhaRBOxFr5WFzmIF+8Q3HaA+K7nyeevFXoe/a67Jxe8Z+KwseB7rpTQ+rhpjvZULXYLw4Yxc6kYKkgfwDYatLHsmf+JlesVT4yAvL/MnK8lAghjGN3hAPDtyiKzHLrRamxCfzQp20SceG0P52MUKZludzN6KXVgvXh3po9ajyMeEtdAvl9X8S9KIMUoF/sXq4AXst2wC+XhUjlbRgSQJH6DD6ksjBfydQwJJlOEY4u9WKnRv7R0d+ABJ4slR02aFbTiKSavjxgrZLDmSKOY6mlloffTbJI5KE1Qs9pEN2INO6yMrD7FVChsjrnG28pCdwaGd2IMN4r2xguRh5GLc+s/zI5uliyRiuslm/mgHjnHk4mHx2ljvI6+jyfqPlMlDbOCweHeYGmYDD1m9zJ23Jrwue0d8suz3Y5+dSfGZLGI9b0hMEmLMcYP1LLIHRx/24X7JlscPkjvwDMowZgeSAp5hL6fF421omr08wwJ7cIyhDM9If2D8IdmI3Wi2VoyyUJRygh3SS2JJk+zgCasFJAuFJM3YjY3inU6BJBX70GWlOH5xxv1nXuBNyZSskOW4yQv82WqGfKGAvQv7kCpe6SxEViMN2Rix+yjkj6zjf6UwxU/xy" +
                                             "H9ZZ++uat5GkI00V05j16Ic5UmcCjaAzt+A7GO8LLmSRTmOyzwWfLC0v4Fvp/CklJE4fSfZidOYtv/+dh6r2O3yvcTHblbZjTfmbRqnsVN2Dj2ufzfiHdQEn4ziby/5if/hXy7ExMe/+B/+FMq+QUyhBu9go1zn6gTJvXgPtaFAksmDPMYW9rsEEx/72cJjPGjvEncBjlq8h3sFDh0h2YIM1FktkV+OST7L2cJ+g9uvvOxnC8uZHxoaxATqkIEtAoe2SlTYggxUYRShuQDzeJRN7OGEQfn3OU6wh008Glq0MW+jqEIGtoiPmbCXbMYbKMeI1U6S2y2HP7GG7RzVPAM/zVG2s4Y/MSd0NOYwgnK8gc3iWSZBcifSUYgBe8nE22c5/sJz7KFXsyFDM/Syh+f4i7U5hyslAQdQiHTcKR5lZlTyOL6GBwzXivgrz/O6BmG8j9d5nr+yiOF/NTz4Go9LxGE6JpuQgdPBH+uxYgdZzrPs5LDDClZmOcxOnmW5temGVh61OY0MbBLvcctOkohn8YO16b9WhtblsIQn2cwuDsYxSpnmILvYzJMsYY7V4W1WJuf+gGeRKDuH2zBZi1TsQZW1iVtWCyDzWMz/YT0vsZdDnIzy3dccJznEXl5iPf+HxcyzW1gYbHpVFfYgVV58cq02KGzC09iPVjs9JdbyKDksZAmP8wzP8Sqv8wZv0hvmUWyWXt7kDV7nVZ7jGR5nCQuZE2oeY6Vejlbsx9NyqBIppbAKW/EastBufV6K/b2lgMUs5S+sZj1b+X/s5H95g8Mc5RgnOMkpTnOGM5zmFCc5wTGOcpg3+F928v/YynpW8xeWspgFkd0nls8eaUcWXsNWKToULcckFbtQBE/0MLl9l8lnEUt4hD/zKMt5nMdZzqP8mUdYwiLmR2N3CIyGB0XYhVRBQ7QSKKl4GwW4Bm/oiUWtbA5eXEMB3pYmJ5F1TJLxEr5Hm/F4tOF7vCTPn4lCw0QhDR+jHD2G7SZz6EE5PkYa5EcWhQlJAtbiHryIz1Ed/BV3x1sPqvE5XsQ9WCuZDVFkg/gkpOBVfIGT0bzvitrd1El8gVeRgiQJwkXRPXbdhQeRjk9wEC3oDqUpK0Y2hW604CA+QToexF1ynBLFek9JxmPYiU+Rh0Z0YtD+wIiI2wwG0YlG5OFT7MRjSJb9QuQEWBLxEJ7Cu/gKxWiABwMYszfMLmTzYQwD8KABxfgK7+IpPIRE+UVEzoVlNe5DGl5ABr5ELk6gDX0YxTi8mA6vHwWzmIYX4xhFH9pwArn4Ehl4AWm4TyaKiPRFZj1S8ASex5vYi8/wHXJwBJWoQyuuwoN+jGDiH3hmMY0JjKAfHlxFK+pQiSPIwXf4DHvxJp7HE0iRN/5io/8HYaz4EjNbjesAAAAZdEVYdFNvZnR3YXJlAEFkb2JlIEltYWdlUmVhZHlxyWU8AAAAAElFTkSuQmCC";

                var bytes = System.Convert.FromBase64String(defaultAvatar);

                var user = new ApplicationUser { UserName = model.Username, Email = model.Email, Avatar = bytes };
                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);

                    // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=320771
                    // Send an email with this link
                    // string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    // var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                    // await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");

                    return RedirectToAction("Index", "Home");
                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var result = await UserManager.ConfirmEmailAsync(userId, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByNameAsync(model.Email);
                if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=320771
                // Send an email with this link
                // string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                // var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);		
                // await UserManager.SendEmailAsync(user.Id, "Reset Password", "Please reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>");
                // return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View();
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await UserManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            AddErrors(result);
            return View();
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/SendCode
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == null)
            {
                return View("Error");
            }
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // Generate the token and send it
            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                return View("Error");
            }
            return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        }

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
                case SignInStatus.Failure:
                default:
                    // If the user does not have an account, then prompt the user to create an account
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new ApplicationUser { UserName = model.Username, Email = model.Email };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager {
            get {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion
    }
}