import { Component, inject, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Title } from '@angular/platform-browser';

@Component({
    template: `
  <div class="container mx-auto text-center select-none">
    <h2>.</h2>
  </div>
    `,
    imports: []
})
export default class AnalyticsComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly title = inject(Title);

  ngOnInit(): void {
    this.route.queryParams.subscribe((params: { [x: string]: any; }) => {
      let version = params['version'];
      if (version !== undefined) {
        this.title.setTitle("Race Element Startup " + version)
      }
    });
  }
}
