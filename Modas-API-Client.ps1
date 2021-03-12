# This code is used to test the Modas-API.
# Ken Kazinski
# Assignment Module 

# $QueryUri = 'https://localhost:5001/api/event/pagesize/10/page/1'

$BaseUri = 'https://modas-kkazinski.azurewebsites.net/'
# $BaseUri = 'https://awsmodasapi.azurewebsites.net/'
# $BaseUri = 'https://localhost:5001/'

# ----------------------------------------------------------------------------
# ----------------------------------------------------------------------------
function Remove-Event {

   param([string]$BaseUri, [int]$PageNumber, [string]$Token, [int] $Index, [int]$EventId, [int] $RecordsOnPage)

   Write-Host
   Write-Host -NoNewline ("Delete index $Index, Event ID " + $EventId + "? (Y=Yes) ")

   $Host.UI.RawUI.Flushinputbuffer()
   $KeyPress = $host.UI.RawUI.ReadKey("NoEcho, IncludeKeyDown")
   $Host.UI.RawUI.Flushinputbuffer()
   Write-Host

   $Key = $KeyPress.Character
   if ($Key -ieq "Y") {

      $HttpHeaders = New-Object "System.Collections.Generic.Dictionary[[String],[String]]"
      $HttpHeaders.Add("Cache-Control", "no-cache")
      $HttpHeaders.Add("X-Requested-With", "PowerShell")
      $HttpHeaders.Add("Authorization", "Bearer " + $Token)
      
      $QueryUri = $BaseUri + "api/event/" + $EventId
   
      # try {
         Invoke-RestMethod -Method 'Delete' -Headers $HttpHeaders -Uri $QueryUri

         if ($RecordsOnPage -eq 1)
         {
            $PageNumber = $PageNumber - 1
         }
      # }
      # catch {
      #     Write-Host "Delete failed."  
      # }      
   }

   return [int]$PageNumber
}

# ----------------------------------------------------------------------------
# ----------------------------------------------------------------------------
function Get-PageData {

   param([string]$BaseUri, [int]$PageNumber, [string]$Token)

   $HttpHeaders = New-Object "System.Collections.Generic.Dictionary[[String],[String]]"
   $HttpHeaders.Add("Cache-Control", "no-cache")
   $HttpHeaders.Add("X-Requested-With", "PowerShell")
   $HttpHeaders.Add("Authorization", "Bearer " + $Token)

   $QueryUri = $BaseUri + 'api/event/pagesize/10/page/' + $PageNumber
   Write-Host("Invoke-RestMethod")
   $ApiData = Invoke-RestMethod -Method Get -Headers $HttpHeaders -Uri $QueryUri

   return $ApiData

}

# ----------------------------------------------------------------------------
# ----------------------------------------------------------------------------
function Invoke-LogUserIn ($BaseUri) {
   # Need to get the token first
   Write-Host
   $UserName = Read-Host "What is your username?  "
   $PassWord = Read-Host "What is your password?  " #-AsSecureString

   $HttpTokenHeaders = New-Object "System.Collections.Generic.Dictionary[[String],[String]]"
   $HttpTokenHeaders.Add("Content-Type", "application/json")
   $body = "{ `"username`": `"" + $UserName + "`", `"password`": `"" + $PassWord + "`"}"

   $QueryUri = $BaseUri + 'api/Token'

   $TokenData = Invoke-RestMethod -Method 'POST' -Headers $HttpTokenHeaders -Uri $QueryUri -Body $Body

   return $TokenData
}

# ----------------------------------------------------------------------------
# ----------------------------------------------------------------------------
function Update-EventFlag {
   
   param ([string]$BaseUri, [string]$Token, [int]$EventId, [bool]$EventFlag)

   $HttpPatchHeaders = New-Object "System.Collections.Generic.Dictionary[[String],[String]]"
   $HttpPatchHeaders.Add("Content-Type", "application/json")
   $HttpPatchHeaders.Add("Authorization", "Bearer " + $TokenData.token)
  
   $QueryUri = $BaseUri + "api/event/" + $Events[$Index].id

   # {"op" : "replace", "path" : "Flagged", "value" : true}
   if ([bool]$Events[$Index].flag) {
      $Body = "[`n {`"op`" : `"replace`", `"path`" : `"Flagged`", `"value`" : false} `n]`n"

   }
   else {
      $Body = "[`n {`"op`" : `"replace`", `"path`" : `"Flagged`", `"value`" : true} `n] `n"
   }

   $Response = Invoke-RestMethod -Method 'PATCH' -Headers $HttpPatchHeaders -Uri $QueryUri -Body $Body
   $Response | ConvertTo-Json
   Write-Host($Response)

}

# ----------------------------------------------------------------------------
# Start of Script
# ----------------------------------------------------------------------------
$TokenData = Invoke-LogUserIn ($BaseUri)
if (!$TokenData) {
   # Check for null
   Write-Host("Failed to log in.")
   exit(0);
}

$GetPage = 1
$Done = $false

do {
   $ApiData = Get-PageData -baseUri $BaseUri -Pagenumber $GetPage -Token $TokenData.token

   # Get the event information from the returned API data
   # An event = @{id=1002; stamp=2021-02-04T18:58:14; flag=False; loc=Family Room}
   $Events = $ApiData.Events

   # Get the page information from the returned API data
   # Page Info = @{totalItems=422; itemsPerPage=10; currentPage=1; totalPages=43; previousPage=1; nextPage=2; rangeStart=1; rangeEnd=10}
   $PageInfo = $ApiData.pagingInfo

   clear-host

   # Display the events on the screen
   Write-Host(" #  Flagged  Date               Time      Location")
   for ($LCV = 0; $LCV -lt $Events.length; $LCV++) {
      Write-Host -NoNewline ("{0,2}" -f $LCV)
      if ([bool]$Events[$LCV].flag) { $Flagged = "Yes" } else { $Flagged = "No" }
      Write-Host -NoNewline ($Flagged.padleft(7).PadRight(11))
      Write-Host -NoNewline (([datetime]$Events[$LCV].stamp).ToString("ddd, MMM dd, yyyy  hh:mm tt  "))
      Write-Host ($Events[$LCV].loc + " " + $Events[$LCV].id)
   }

   # Set the inital value for the allowed key press values
   $AllowedKeys = "Ee0)1!2@3#4`$5%6^7&8*9(".Substring(0, ($Events.length * 2) + 2) 

   Write-Host
   if ($PageInfo.currentPage -gt 1) { $DisplayValue = "(F)irst"; $AllowedKeys = $AllowedKeys + "Ff" } else { $DisplayValue = "" }
   Write-Host -NoNewline ($DisplayValue.PadLeft(10))

   if ($PageInfo.currentPage -gt 1) { $DisplayValue = "(P)revious"; $AllowedKeys = $AllowedKeys + "Pp" } else { $DisplayValue = "" }
   Write-Host -NoNewline ($DisplayValue.PadLeft(13))

   Write-Host -NoNewline ("   $($PageInfo.currentPage) - $($PageInfo.itemsPerPage) of $($PageInfo.totalItems)")

   if ($PageInfo.currentPage -lt $PageInfo.totalPages) { $DisplayValue = "(N)ext"; $AllowedKeys = $AllowedKeys + "Nn" } else { $DisplayValue = "" }
   Write-Host -NoNewline ($DisplayValue.PadLeft(8))

   if ($PageInfo.currentPage -lt $PageInfo.totalPages) { $DisplayValue = "(L)ast"; $AllowedKeys = $AllowedKeys + "Ll" } else { $DisplayValue = "" }
   Write-Host -NoNewline ($DisplayValue.PadLeft(8))

   Write-Host ("  (E)xit.")
   Write-Host
   Write-Host ("Press a number to toggle a flagged value or a letter, to delete a item use the shift-number key ")

   $Host.UI.RawUI.Flushinputbuffer()
   $KeyPress = $host.UI.RawUI.ReadKey("NoEcho, IncludeKeyDown")
   $Host.UI.RawUI.Flushinputbuffer()

   # Write-Host($KeyPress.Key)
   If ($AllowedKeys.Contains($KeyPress.Character)) {
      $Key = $KeyPress.Character

      switch ($Key) {
         "E" { $Done = $true; break }
         "F" { $GetPage = 1; break }
         "L" { $GetPage = $PageInfo.totalPages; break }
         "N" { $GetPage = $PageInfo.nextPage; break }
         "P" { $GetPage = $PageInfo.previousPage; break }
         { ')', '!', '@', '#', '$', '%', '^', '&', '*' -contains $_ } {
            switch ($Key) {
               ")" { $Index = 0; break }
               "!" { $Index = 1; break }
               "@" { $Index = 2; break }
               "#" { $Index = 3; break }
               "$" { $Index = 4; break }
               "%" { $Index = 5; break }
               "^" { $Index = 6; break }
               "&" { $Index = 7; break }
               "*" { $Index = 8; break }
               "(" { $Index = 9; break }
            }
            $GetPage = Remove-Event -baseUri $BaseUri -Token $TokenData.token -PageNumber $GetPage -Index $Index -EventId $Events[$Index].id -RecordsOnPage $Events.length
            break 
         }
         Default {
            $Index = [int]::Parse($Key)
            Update-EventFlag -baseUri $BaseUri -Token $TokenData.token -EventId $Events[$Index].id -EventFlag $Events[$Index].flag
         }
      }
   }
} until ($Done -eq $true)
