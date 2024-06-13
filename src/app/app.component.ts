import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { SocialsComponent } from "./components/socials/socials.component";

@Component({
  selector: 'app-root',
  standalone: true,
  template: `
    <div class="container mx-auto flex flex-col self-center pt-2 select-none">

      <div class="mx-auto mb-3">
        <a href="/" class="self-center drop-shadow-[0_35px_35px_rgba(1,1,1,0.25)] text-center font-['Conthrax'] text-5xl md:text-7xl text-[orangered] hover:text-[orangered] ">
          <div class="-skew-x-12 ">Race Element</div>
        </a>
        <p class="text-center font-['Conthrax']">Provides Solutions for Sim Racing</p>
      </div>

      <div class="border-t-2 border-[orangered] mb-2 container mx-auto max-w-md md:max-w-xl" ></div>

      <div class="mx-auto container place-content-center grid max-w-xl md:max-w-3xl grid-cols-2 md:grid-cols-1">

        <nav class="select-none text-lg font-['Conthrax'] mx-auto place-content-start md:place-content-center md:text-2xl flex flex-wrap flex-col md:flex-row">
          <a href="/news" class="mx-auto text-center mb-2 md:mb-0 text-[white] hover:text-[red] hover:bg-[rgba(0,0,0,0.8)] ml-1 mr-1 pl-2 pr-2 bg-[rgba(0.2,0.2,0.2)] rounded-tl-lg rounded-br-lg">News</a>
          <a href="/guide" class="mx-auto text-center mb-2 md:mb-0 text-[white] hover:text-[red] hover:bg-[rgba(0,0,0,0.8)] ml-1 mr-1 pl-2 pr-2 bg-[rgba(0.2,0.2,0.2)] rounded-tl-lg rounded-br-lg">Guides</a>
          <a href="/guide/features" class="mx-auto text-center mb-2 md:mb-0 text-[white] hover:text-[red] hover:bg-[rgba(0,0,0,0.8)] ml-1 mr-1 pl-2 pr-2 bg-[rgba(0.2,0.2,0.2)] rounded-tl-lg rounded-br-lg">Features</a>
          <a href="/guide/how-to-get-started" class="mx-auto text-center mb-2 md:mb-0 text-[white] hover:text-[red] hover:bg-[rgba(0,0,0,0.8)] ml-1 mr-1 pl-2 pr-2 bg-[rgba(0.2,0.2,0.2)] rounded-tl-lg rounded-br-lg">Download</a>
        </nav>

        <app-socials class="mx-auto" />
      </div>

      <div class="border-t-2 border-[orangered] mt-1 md:mt-2 container mx-auto max-w-md md:max-w-xl" ></div>
      <router-outlet class="mt-4 md:mt-4"></router-outlet>

      <br>

      <footer class="text-sm select-none mx-auto">
        <img class="mx-auto" src="https://hits.seeyoufarm.com/api/count/keep/badge.svg?url=https%3A%2F%2Fgithub.com%2FRiddleTime%2FRace-Element&amp;count_bg=%23FF4500&amp;title_bg=%23555555&amp;icon=&amp;icon_color=%23E7E7E7&amp;title=Usage%3A+Today+%2F+All-time&amp;edge_flat=false">
        <p class="text-center">© {{ThisYear}} Reinier Klarenberg</p>
      </footer>
    </div>
  `,
  styles: [
    `
    `,
  ],
  imports: [RouterOutlet, SocialsComponent]
})
export class AppComponent {
ThisYear:number = new Date().getFullYear();

}
